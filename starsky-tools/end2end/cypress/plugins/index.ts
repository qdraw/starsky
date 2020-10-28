import path from 'path'
import fs from 'fs'

// Get enviromental JSON files
function getConfigurationByFile (file: string, folder: string) {
  const pathToConfigFile = path.resolve('cypress', 'config', `${folder}`, `${file}.json`)

  const rawdata = fs.readFileSync(pathToConfigFile, 'utf8')
  return JSON.parse(rawdata)
}

module.exports = (_, config) => {
  // accept a configEnv value or use development by default
  const file = config.env.configEnv || 'local'
  const folder = config.env.configFolder || 'starsy'

  // Add Cypress linting to build process
  // on('file:preprocessor', cypressEslint());

  var updatedConfig = getConfigurationByFile(file, folder)
  process.env.E2E_AUTH_USER = updatedConfig.env.AUTH_USER
  process.env.E2E_AUTH_PASS = updatedConfig.env.AUTH_PASS
  // this should update the envs but it isn't working
  return updatedConfig
}
