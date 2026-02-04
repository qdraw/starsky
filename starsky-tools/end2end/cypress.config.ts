import { defineConfig } from 'cypress'
import fs from 'fs'
import path from 'path'

// Get enviromental JSON files
function getConfigurationByFile (file: string, folder: string) {
  const pathToConfigFile = path.resolve('cypress', 'config', `${folder}`, `${file}.json`)

  const rawdata = fs.readFileSync(pathToConfigFile, 'utf8')
  return JSON.parse(rawdata)
}

export default defineConfig({
  reporter: 'junit',
  reporterOptions: {
    mochaFile: 'results/test-output-[hash].xml'
  },
  viewportHeight: 900,
  viewportWidth: 1280,
  screenshotsFolder: 'cypress/screenshots/build',
  blockHosts: [
    '*.applicationinsights.azure.com'
  ],
  e2e: {
    setupNodeEvents (on, config) {
      if (process.env.CYPRESS_BASE_URL && process.env.cypress_name && process.env.cypress_AUTH_USER && process.env.cypress_AUTH_PASS) {
        console.log('ignored due existing env names =>', process.env.CYPRESS_BASE_URL, process.env.cypress_name, process.env.cypress_AUTH_USER, 'cypress_AUTH_PASS')
        return
      }
      console.log('running file based settings')

      // accept a configEnv value or use development by default
      const file = config.env.configEnv || 'local'
      const folder = config.env.configFolder || 'starsky'

      // Add Cypress linting to build process
      // on('file:preprocessor', cypressEslint());

      const updatedConfig = getConfigurationByFile(file, folder)
      return updatedConfig
    },
    excludeSpecPattern: '*.json'
  }
})
