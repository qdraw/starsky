import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('Search -from upload', () => {
  beforeEach('Check some config settings and do them before each test', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()
  })

  it('Go to urlSearchFromUpload page and newest first', () => {
    if (!config.isEnabled) return
    cy.visit(config.urlSearchFromUpload)

    // newest first

    cy.get('.folder > div:nth-child(1)').invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_134151.jpg')

    cy.get('.folder > div:nth-child(2)').invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_112430.jpg')

    cy.get('.folder > div:nth-child(3)').invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_111408.jpg')
  })
})
