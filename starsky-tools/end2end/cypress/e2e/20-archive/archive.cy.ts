import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('Archive (20)', () => {
  beforeEach('Check some config settings and do them before each test (20)', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()
  })

  it('Check if folder is there (20)', () => {
    if (!config.isEnabled) return
    cy.visit(config.url, {
      headers: {
        Connection: 'Keep-Alive'
      }
    })

    cy.get(flow.content)
  })

  it('more button is there (20)', () => {
    cy.visit(config.url, {
      headers: {
        Connection: 'Keep-Alive'
      }
    })
    cy.get(flow.more)
  })
})
