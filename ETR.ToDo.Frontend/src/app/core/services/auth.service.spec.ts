import { TestBed, getTestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { describe, it, expect, beforeEach, afterEach, vi, beforeAll } from 'vitest';
import { AuthService } from './auth.service';
import { StorageService } from './storage.service';
import { AuthLoginRequest } from '../../features/auth/models/auth-login-request.model';
import { AuthLoginResponse } from '../../features/auth/models/auth-login-response.model';
import { environment } from '../../../environments/environment';

// Initialize Angular testing environment
beforeAll(() => {
  getTestBed().initTestEnvironment(
    BrowserDynamicTestingModule,
    platformBrowserDynamicTesting()
  );
});

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let storageService: StorageService;
  let router: Router;

  beforeEach(() => {
    TestBed.resetTestingModule(); // Reset between tests

    const storageMock = {
      getUser: vi.fn(),
      getToken: vi.fn(),
      getRefreshToken: vi.fn(),
      setUser: vi.fn(),
      setToken: vi.fn(),
      setRefreshToken: vi.fn(),
      clear: vi.fn()
    };

    const routerMock = {
      navigate: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService,
        { provide: StorageService, useValue: storageMock },
        { provide: Router, useValue: routerMock }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    storageService = TestBed.inject(StorageService);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    httpMock.verify(); // This will fail if there are pending requests
  });

  // ✅ Test Case 1: Login - Success
  it('should login successfully and store auth data', () => {
    return new Promise<void>((resolve) => {
      const loginRequest: AuthLoginRequest = {
        email: 'test@example.com',
        password: 'password123'
      };

      const mockResponse: AuthLoginResponse = {
        user: {
          id: '123',
          email: 'test@example.com',
          fullName: 'Test User',
          role: 'User'
        },
        token: {
          accessToken: 'mock-access-token',
          refreshToken: 'mock-refresh-token',
          expiresInSeconds: 3600,
          expiresAt: new Date(Date.now() + 3600000).toISOString(),
          refreshTokenExpiresAt: new Date(Date.now() + 7 * 24 * 3600000).toISOString()
        }
      };

      service.login(loginRequest).subscribe({
        next: (response) => {
          expect(response).toEqual(mockResponse);
          expect(storageService.setUser).toHaveBeenCalledWith(mockResponse.user);
          expect(storageService.setToken).toHaveBeenCalledWith(mockResponse.token.accessToken);
          expect(storageService.setRefreshToken).toHaveBeenCalledWith(mockResponse.token.refreshToken);
          expect(service.isAuthenticated()).toBe(true);
          resolve();
        }
      });

      const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(loginRequest);
      req.flush({ data: mockResponse, success: true });
    });
  });

  // ✅ Test Case 2: Login - Failure (401 Unauthorized)
  it('should handle login failure with invalid credentials', () => {
    return new Promise<void>((resolve) => {
      const loginRequest: AuthLoginRequest = {
        email: 'test@example.com',
        password: 'wrongpassword'
      };

      service.login(loginRequest).subscribe({
        error: (error) => {
          expect(error.status).toBe(401);
          expect(service.isAuthenticated()).toBe(false);
          resolve();
        }
      });

      const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/login`);
      req.flush({ message: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  // ✅ Test Case 3: Logout - Clear storage and navigate
  it('should logout and clear all stored data', () => {
    vi.mocked(storageService.getRefreshToken).mockReturnValue('mock-refresh-token');

    service.logout();

    // Expect HTTP POST to revoke endpoint
    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/revoke`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ refreshToken: 'mock-refresh-token' });
    req.flush({}); // Respond with empty success

    expect(storageService.clear).toHaveBeenCalled();
    expect(service.currentUser()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  // ✅ Test Case 4: Get Token
  it('should retrieve stored token', () => {
    vi.mocked(storageService.getToken).mockReturnValue('mock-access-token');

    const token = service.getToken();

    expect(token).toBe('mock-access-token');
    expect(storageService.getToken).toHaveBeenCalled();
  });

  // ✅ Test Case 5: Register - Success
  it('should register successfully and auto-login', () => {
    return new Promise<void>((resolve) => {
      const registerRequest = {
        email: 'new@example.com',
        password: 'password123',
        firstName: 'New',
        lastName: 'User'
      };

      const mockResponse: AuthLoginResponse = {
        user: { id: '456', email: 'new@example.com', fullName: 'New User', role: 'User' },
        token: {
          accessToken: 'new-token',
          refreshToken: 'new-refresh',
          expiresInSeconds: 3600,
          expiresAt: new Date(Date.now() + 3600000).toISOString(),
          refreshTokenExpiresAt: new Date(Date.now() + 7 * 24 * 3600000).toISOString()
        }
      };

      service.register(registerRequest).subscribe({
        next: (response) => {
          expect(response).toEqual(mockResponse);
          expect(service.isAuthenticated()).toBe(true);
          resolve();
        }
      });

      const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/register`);
      expect(req.request.method).toBe('POST');
      req.flush({ data: mockResponse, success: true });
    });
  });
});
