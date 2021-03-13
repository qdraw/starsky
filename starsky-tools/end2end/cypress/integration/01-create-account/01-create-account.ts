import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('Create Account', () => {
  beforeEach('Check some config settings and do them before each test', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      cy.log('not enabled for this env')
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    // Check for a valid statuscode, otherwise skip test cases
    cy.checkStatusCode(config.url)
  })

  it('register page is open', () => {
    if (!config.isEnabled) return false
    cy.checkStatusCode(config.urlStatusApi)
    // When failing set in application the setting to
    // app__IsAccountRegisterOpen to true
  })

  it('does the register page for and checks result page', () => {
    if (!config.isEnabled) return false
    cy.checkStatusCode(config.urlStatusApi)

    /* Start flow (connection header prevents script from
        occassionaly throwing ESOCKETTIMEDOUT errors in CI) */
    cy.visit(config.url, {
      headers: {
        Connection: 'Keep-Alive'
      }
    })

    cy.get(flow.form).within(() => {
      cy.get(flow.fields.name).type(Cypress.env('AUTH_USER'))
      cy.get(flow.fields.password).type(Cypress.env('AUTH_PASS'))
      cy.get(flow.fields.confirmPassword).type(Cypress.env('AUTH_PASS'))
      cy.get(flow.fields.submit)
        .click()
        .url()
        .should('contain', config.successUrl)
    })
  })
})
