import { Page } from '@playwright/test';

export async function login(page: Page, username: string, password: string) {
  await page.goto('http://localhost:4000/starsky/account/login');
  await page.fill('input[name="username"]', username);
  await page.fill('input[name="password"]', password);
  await Promise.all([
    await page.waitForURL('**/dashboard', { timeout: 10000 }), // Adjust URL as needed
    page.click('button[type="submit"]'),
  ]);
}
