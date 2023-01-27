import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName] as any

describe('Login (02)', () => {
  beforeEach('Check some config settings and do them before each test (02)', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()
  })

  it('login page is here (02)', () => {
    if (!config.isEnabled) return false
    cy.checkStatusCode(config.url)
  })

  it('does login into app (02)', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return false

    cy.intercept('/starsky/api/account/status').as('status')

    /* Start flow (connection header prevents script from
        occassionaly throwing ESOCKETTIMEDOUT errors in CI) */
    cy.visit(config.url, {
      headers: {
        Connection: 'Keep-Alive'
      }
    })
    cy.wait('@status')

    cy.get(flow.form).within(() => {
      cy.get(flow.fields.name).type(Cypress.env('AUTH_USER'))
      cy.get(flow.fields.password).type(Cypress.env('AUTH_PASS'))
      cy.get(flow.fields.submit)
        .click()
        .url()
        .should('contain', config.successUrl)
    })
  })
})
