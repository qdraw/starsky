import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
const config = configFile[envFolder][envName]

describe('env (01/03)', () => {
  it('check env file (01/03)', () => {
    if (!config.isEnabled) return false

    cy.sendAuthenticationHeader()

    cy.visit(config.env, {
      headers: {
        'x-force-html': true
      }
    })
  })
})
