import { defineConfig } from 'cypress'
import fs from 'fs'
import path from 'path'

// Get enviromental JSON files
function getConfigurationByFile (file: string, folder: string) {
  const pathToConfigFile = path.resolve('cypress', 'config', `${folder}`, `${file}.json`)

  const rawdata = fs.readFileSync(pathToConfigFile, 'utf8')
  return JSON.parse(rawdata)
}

type ApiSession = { baseUrl: string; headers: Record<string, string> }

function collectCookies (response: Response, jar: Map<string, string>) {
  for (const cookie of response.headers.getSetCookie()) {
    const [pair] = cookie.split(';')
    const index = pair.indexOf('=')
    jar.set(pair.slice(0, index), pair.slice(index + 1))
  }
}

async function login (config: Cypress.PluginConfigOptions): Promise<ApiSession | null> {
  const baseUrl = config.baseUrl
  if (!baseUrl || !config.env.AUTH_USER || !config.env.AUTH_PASS) return null

  const jar = new Map<string, string>()
  const cookieHeader = () => Array.from(jar.entries()).map(([name, value]) => `${name}=${value}`).join('; ')

  collectCookies(await fetch(`${baseUrl}/account/login`, { redirect: 'manual' }), jar)
  const response = await fetch(`${baseUrl}/starsky/api/account/login`, {
    method: 'POST',
    redirect: 'manual',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
      'X-XSRF-TOKEN': jar.get('X-XSRF-TOKEN') ?? '',
      Cookie: cookieHeader()
    },
    body: new URLSearchParams({ Email: config.env.AUTH_USER, Password: config.env.AUTH_PASS })
  })
  if (!response.ok) return null
  collectCookies(response, jar)
  return { baseUrl, headers: { Cookie: cookieHeader() } }
}

async function setRateLimiting (session: ApiSession, value: boolean) {
  const response = await fetch(`${session.baseUrl}/api/env`, {
    method: 'POST',
    headers: { ...session.headers, 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({ useRateLimiting: String(value) })
  })
  return response.ok
}

// Every test logs in again, which the login rate limit (30 per minute per IP) blocks on a full run.
// So switch the limiter off before the first test and put the original value back after the last one.
function toggleRateLimitingForRun (on: Cypress.PluginEvents, config: Cypress.PluginConfigOptions) {
  let restoreTo: boolean | null = null

  on('before:run', async () => {
    try {
      const session = await login(config)
      if (!session) return
      const env = await (await fetch(`${session.baseUrl}/api/env`, { headers: session.headers })).json()
      if (env.useRateLimiting === false) return
      if (await setRateLimiting(session, false)) restoreTo = true
    } catch (error) {
      console.log('could not disable rate limiting', error)
    }
  })

  on('after:run', async () => {
    if (restoreTo === null) return
    try {
      const session = await login(config)
      if (session) await setRateLimiting(session, restoreTo)
    } catch (error) {
      console.log('could not restore rate limiting', error)
    }
  })
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
        toggleRateLimitingForRun(on, config)
        return
      }
      console.log('running file based settings')

      // accept a configEnv value or use development by default
      const file = config.env.configEnv || 'local'
      const folder = config.env.configFolder || 'starsky'

      // Add Cypress linting to build process
      // on('file:preprocessor', cypressEslint());

      const updatedConfig = getConfigurationByFile(file, folder)
      toggleRateLimitingForRun(on, { ...config, ...updatedConfig, env: { ...config.env, ...updatedConfig.env } })
      return updatedConfig
    },
    excludeSpecPattern: '*.json'
  }
})
