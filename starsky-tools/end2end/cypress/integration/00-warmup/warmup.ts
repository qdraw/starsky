import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
const config = configFile[envFolder][envName]

describe('Warmup', () => {
  it('Warmup before starting', {
    retries: {
      runMode: 10,
      openMode: 10
    }
  }, () => {
    if (!config.isEnabled) return false
    cy.request(config.url, {
      failOnStatusCode: false,
      retryOnNetworkFailure: true,
      log: true
    }).then((status) => {
      console.log(status)
    })
  })
})
