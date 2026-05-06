# ETR.ToDo.Tests.Playwright - E2E Tests

Playwright end-to-end tests for the ETR.ToDo Angular frontend application.

**Integrated with Visual Studio solution** - appears in Solution Explorer!

---

## 📦 Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| Node.js | 20.x LTS | Required |
| Angular Frontend | Running | Must be at `http://localhost:4200` |
| Backend API | Running | Must be at `http://localhost:5214` |

---

## 🚀 Setup

### Option 1: Via Visual Studio (Recommended)

1. **Open Solution** in Visual Studio
2. **Right-click** on `ETR.ToDo.Tests.Playwright` project
3. **Select "Build"** - This will:
   - ✅ Run `npm install`
   - ✅ Install Playwright browsers
   - ✅ Run the tests

### Option 2: Via Command Line

```powershell
cd ETR.ToDo.Tests.Playwright
npm install
npx playwright install chromium
```

---

## 🧪 Running Tests

### Via Visual Studio

1. **Start Frontend First:**
   ```powershell
   cd ETR.ToDo.Frontend
   npm start
   ```

2. **In Visual Studio:**
   - Right-click `ETR.ToDo.Tests.Playwright` project
   - Select **"Build"** or **"Rebuild"**
   - Tests run automatically after build

### Via Command Line

```powershell
# Run all tests (headless)
npm test

# Run with UI mode (interactive)
npm run test:ui

# Run in headed mode (see browser)
npm run test:headed

# Debug mode (step through tests)
npm run test:debug

# View HTML report
npm run report
```

---

## 📁 Project Structure

```
ETR.ToDo.Tests.Playwright/
├── tests/
│   └── signin-modal.spec.ts         # Test: Sign in modal opens
├── ETR.ToDo.Tests.Playwright.csproj  # MSBuild wrapper for VS
├── playwright.config.ts              # Playwright configuration
├── package.json
├── tsconfig.json
└── README.md
```

---

## ✅ Current Tests

### signin-modal.spec.ts
**Test:** Opens app and clicks "Sign in" button

**Verifies:**
- ✅ Sign in button is clickable
- ✅ Login dialog/modal appears
- ✅ Email and password fields are visible

---

## 🔧 Visual Studio Integration

The `.csproj` file provides MSBuild integration:

**What happens when you build:**
1. `NpmInstall` target → runs `npm install`
2. `PlaywrightInstall` target → installs Chromium browser
3. `RunPlaywrightTests` target → runs `npm test`

**Disable auto-run tests:**  
Comment out the `RunPlaywrightTests` target in `.csproj` if you only want to install dependencies without running tests.

---

## 🐛 Troubleshooting

### Issue: "Navigation timeout"

**Cause:** Frontend not running at `http://localhost:4200`

**Solution:**
```powershell
cd ..\ETR.ToDo.Frontend
npm start
```

### Issue: "Chromium not installed"

**Solution:**
```powershell
npx playwright install chromium
```

### Issue: "Test fails - button not found"

**Cause:** Frontend routing or button text changed

**Solution:** Check the button text in `src/app/shared/components/app-navigation/`

---

## 📚 Resources

- [Playwright Documentation](https://playwright.dev/)
- [Playwright Testing Guide](https://playwright.dev/docs/writing-tests)
- [Locators Best Practices](https://playwright.dev/docs/locators)
