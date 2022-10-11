import { defineConfig } from 'cypress'

export default defineConfig({
  reporter: 'junit',
  reporterOptions: {
    mochaFile: 'results/test-output-[hash].xml',
  },
  viewportHeight: 900,
  viewportWidth: 1280,
  experimentalFetchPolyfill: true,
  screenshotsFolder: 'cypress/screenshots/build',
  e2e: {
    setupNodeEvents(on, config) {},
    excludeSpecPattern: '*.json',
  },
})
