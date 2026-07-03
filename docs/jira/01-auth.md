# Epic: Authentication

**Summary:** Implement dialog-based authentication including login, registration, JWT access tokens, refresh token rotation, and idle session management. No route-based auth exists — all auth flows are dialog-only.

---

## Story 1.1 — User Login

**As an** unauthenticated user  
**I want to** log in via a dialog  
**So that** I can access my task lists

### Task 1.1.1 — Backend: Login Endpoint

**[DEV-BE]** Create `POST /api/auth/login` endpoint
- `LoginRequestDto` with email + password
- `AuthenticationService.LoginAsync()` — BCrypt verify
- `JwtTokenService.GenerateToken()` — 60 min JWT with sub, email, role, exp claims
- `RefreshTokenService.CreateAsync()` — 7-day rotating refresh token
- Return `LoginResponseDto` with accessToken, refreshToken, user
- `MapInboundClaims = false` in `AddJwtBearer`

**[DEV-BE]** Create `AuthController` — thin, delegates to `AuthenticationService`

**[QA-UNIT]** Unit test `AuthenticationService.LoginAsync`
- Happy path: valid credentials returns tokens
- Wrong password: throws `UnauthorizedAccessException`
- Unknown email: throws `KeyNotFoundException`

**[QA-BDD]** BDD feature: `Authentication.Login.feature`
- Scenario: valid credentials → 200 + tokens returned
- Scenario: wrong password → 401
- Scenario: unknown email → 404

### Task 1.1.2 — Frontend: Login Dialog

**[DEV-FE]** Create `AuthLoginDialogComponent` (lazy-loaded via dynamic `import()`)
- Separate `.html` and `.scss` files
- `inject()` for all DI
- `signal()` for loading state
- Error UX pattern: shake + warn colour + snackbar on invalid credentials
- `FormControl` with `noWhitespace` validator on email + password

**[DEV-FE]** Create `AuthDialogService` in `core/services/`
- `openLogin()` — lazy-loads and opens `AuthLoginDialogComponent`
- `openRegister()` — lazy-loads and opens `AuthRegisterDialogComponent`

**[DEV-FE]** Create `AuthService` in `core/services/`
- `login()` — calls API, stores token + user via `StorageService`, sets `_currentUser` signal
- `isAuthenticated = computed(() => !!_currentUser())`
- `currentUser` — readonly signal

**[QA-FE]** Vitest: `auth.service.spec.ts`
- `isAuthenticated` returns false when no user stored
- `isAuthenticated` returns true after login success
- `logout()` clears storage and navigates to `/`

**[QA-E2E]** Playwright: `signin-modal.spec.ts`
- Login dialog opens on sign-in button click
- Valid credentials → dialog closes, user redirected to `/todos`
- Invalid credentials → snackbar appears, fields shake

---

## Story 1.2 — User Registration

**As an** unauthenticated user  
**I want to** register a new account via a dialog  
**So that** I can start using the app

### Task 1.2.1 — Backend: Register Endpoint

**[DEV-BE]** Create `POST /api/auth/register`
- `RegisterRequestDto` — name (max 100), email (max 256), password (min 8)
- `AuthenticationService.RegisterAsync()` — BCrypt hash, duplicate email check
- Return same `LoginResponseDto` as login (auto-login on register)

**[QA-UNIT]** Unit test `AuthenticationService.RegisterAsync`
- Happy path: new user created, tokens returned
- Duplicate email: throws `InvalidOperationException`
- Weak password: validation rejects before service

**[QA-BDD]** BDD feature: `Authentication.Register.feature`
- Scenario: valid registration → 201 + auto-logged in
- Scenario: duplicate email → 400
- Scenario: password too short → 400

### Task 1.2.2 — Frontend: Register Dialog

**[DEV-FE]** Create `AuthRegisterDialogComponent` (lazy-loaded)
- Name, email, password, confirm password fields
- `passwordMatch` validator on confirm password
- `noWhitespace` validator on name
- Error UX pattern on all fields
- On success: closes dialog, navigates to `/todos`

**[QA-FE]** Vitest: `no-whitespace.validator.spec.ts`, `password-match.validator.spec.ts`

**[QA-E2E]** Playwright: `register-modal.spec.ts`
- Register dialog opens from login dialog link
- Valid data → dialog closes, redirected to `/todos`
- Mismatched passwords → error shown

---

## Story 1.3 — JWT Refresh Token Rotation

**As an** authenticated user  
**I want** my session automatically refreshed  
**So that** I am not logged out unexpectedly after 60 minutes

### Task 1.3.1 — Backend: Refresh Token Endpoint

**[DEV-BE]** Create `POST /api/auth/refresh-token`
- Validate refresh token (not expired, not revoked)
- Issue new JWT + new refresh token
- Revoke old refresh token (rotation)
- Return 401 if token invalid/expired

**[QA-UNIT]** Unit test `RefreshTokenService`
- Happy path: valid token → new tokens issued, old revoked
- Expired token → throws `UnauthorizedAccessException`
- Revoked token → throws `UnauthorizedAccessException`

**[QA-BDD]** BDD feature: `Authentication.RefreshToken.feature`
- Scenario: valid refresh token → 200 + new tokens
- Scenario: expired token → 401
- Scenario: reused revoked token → 401

### Task 1.3.2 — Frontend: Auth Interceptor

**[DEV-FE]** Create `authInterceptor` in `core/interceptors/`
- Attach `Authorization: Bearer {token}` to every request
- On 401 → call `AuthService.handleUnauthorized()` → POST refresh
- On refresh success → retry original request with new token
- On refresh fail → logout + open login dialog

**[QA-FE]** Vitest: `auth.interceptor.spec.ts`
- Token attached to outgoing requests
- 401 triggers refresh attempt
- Refresh failure triggers logout

---

## Story 1.4 — Idle Session Detection

**As an** authenticated user  
**I want** to be warned when my session is about to expire due to inactivity  
**So that** I can choose to extend it or be safely logged out

### Task 1.4.1 — Backend: Config Endpoint

**[DEV-BE]** Create `GET /api/config` — returns `AppConfigDto`
- `idleTimeoutMinutes` — from `appsettings.json Session:IdleTimeoutMinutes`
- `warningCountdownSeconds` — from `appsettings.json Session:WarningCountdownSeconds`
- `supportEmail` — from `appsettings.json`

**[QA-BDD]** BDD: config endpoint returns expected shape

### Task 1.4.2 — Frontend: Idle Detection + Warning Dialog

**[DEV-FE]** Create abstract `IdleService` + `NgIdleService` implementation (wraps `@ng-idle/core`)
- Started on login, stopped on logout
- Timeout values from `ConfigService` — never hardcoded

**[DEV-FE]** Create `SessionWarningDialogComponent` (lazy-loaded)
- Shows live countdown (disableClose: true)
- "Stay logged in" → resets idle timer
- Countdown expires → logout + login dialog

**[DEV-FE]** Wire `appInitializer` — calls `GET /api/config` before bootstrap, populates `ConfigService`

**[QA-FE]** Vitest: `config.service.spec.ts` — config stored correctly, defaults used on failure
