import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('env', () => {
  it('check env file', () => {
    if (!config.isEnabled) return false

    cy.sendAuthenticationHeader()

    cy.visit(config.env, {
        headers: {
            "x-force-html": true
        }
    });

  })

})
