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

    // clean cache before running
    cy.request('POST', config.searchClearCache)
  })

  it('Go to urlSearchFromUpload page and newest first', () => {
    if (!config.isEnabled) return

    cy.intercept('/search?t=-inurl:starsky-end2end-test%20-imageformat:jpg').as('search')
    cy.visit(config.urlSearchFromUpload)
    cy.wait('@search')

    // newest first

    cy.get('.folder > div:nth-child(1)').invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_134151.jpg')

    cy.get('.folder > div:nth-child(2)').invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_112430.jpg')

    cy.get('.folder > div:nth-child(3)').invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_111408.jpg')
  })
})
