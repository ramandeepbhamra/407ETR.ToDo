import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test('should open signin modal when clicking Sign in button', async ({ page }) => {
    // Navigate to the app
    await page.goto('/');

    // Wait for the page to load
    await page.waitForLoadState('networkidle');

    // Take screenshot for debugging
    await page.screenshot({ path: 'test-results/homepage.png', fullPage: true });

    // Click the "Sign in" button using its ID
    const signInButton = page.locator('#main-nav-sign-in');
    await signInButton.waitFor({ state: 'visible', timeout: 10000 });
    await signInButton.click();

    // Verify the login dialog/modal is visible
    await expect(page.getByRole('dialog')).toBeVisible();

    // Verify the dialog contains login form elements using formControlName
    await expect(page.locator('[formControlName="email"]')).toBeVisible();
    await expect(page.locator('[formControlName="password"]')).toBeVisible();
  });
});
