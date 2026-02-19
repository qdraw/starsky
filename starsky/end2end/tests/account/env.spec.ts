import { test, expect } from '@playwright/test';
import { login } from '../helpers/loginHelper';
  
test('log app config from body', async ({ page }) => {
  // Login first
  await login(page, 'yourUsername', 'yourPassword');
  await page.goto('http://localhost:4000/starsky/api/env');
  const bodyText = await page.locator('body').innerText();
  console.log('Raw body text:', bodyText);
  const parsedData = JSON.parse(bodyText);

  console.log(`appVersion: ${parsedData.appVersion}`);
  console.log(`appVersionBuildDateTime: ${parsedData.appVersionBuildDateTime}`);
  console.log(`databaseType: ${parsedData.databaseType}`);
  console.log(`addMemoryCache: ${parsedData.addMemoryCache}`);
  console.log(`isAccountRegisterOpen: ${parsedData.isAccountRegisterOpen}`);
  console.log(`accountRegisterDefaultRole: ${parsedData.accountRegisterDefaultRole}`);
  console.log(`enablePackageTelemetry: ${parsedData.enablePackageTelemetry}`);
  console.log(`enablePackageTelemetryDebug: ${parsedData.enablePackageTelemetryDebug}`);
  console.log(`useDiskWatcherIntervalInMilliseconds: ${parsedData.useDiskWatcherIntervalInMilliseconds}`);
  console.log(`cpuUsageMaxPercentage: ${parsedData.cpuUsageMaxPercentage}`);
  console.log(`thumbnailGenerationIntervalInMinutes: ${parsedData.thumbnailGenerationIntervalInMinutes}`);
  console.log(`geoFilesSkipDownloadOnStartup: ${parsedData.geoFilesSkipDownloadOnStartup}`);
  console.log(`exiftoolSkipDownloadOnStartup: ${parsedData.exiftoolSkipDownloadOnStartup}`);
  console.log(`useSystemTrash: ${parsedData.useSystemTrash}`);
});