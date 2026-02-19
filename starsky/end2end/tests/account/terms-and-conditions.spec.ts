import { test, expect } from '@playwright/test';

test('test terms and conditions', async ({ page }) => {
  await page.goto('http://localhost:4000/account/register');
  await page.locator('[data-test="toc"]').click();
  await  page.locator('[data-test="terms-and-coditions"]').click();
  await  page.locator('[data-test="home-link"]').click();
  // Assert that the user is back on the home page
  await expect(page).toHaveURL('http://localhost:4000/starsky/');
});