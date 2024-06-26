import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('Search (30)', () => {
  beforeEach('Check some config settings and do them before each test (30)', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()
  })

  it('Type and go to search page (30)', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)

    cy.intercept('/starsky/api/search*').as('search')

    cy.get(flow.fields.search).type(flow.fields.searchData)
      .url()
      .should('contain', flow.successUrl)

    cy.wait('@search')
    cy.get(flow.boxContent)
  })

  it('Go Direct to search page (30)', () => {
    if (!config.isEnabled) return
    cy.visit(config.urlSearchJpg)
    cy.get(flow.boxContent)
  })

  it('Navigate to first detailview item (30)', () => {
    if (!config.isEnabled) return
    cy.visit(config.urlSearchJpg)
    cy.get(flow.boxContent).first().click()
      .url()
      .should('contain', flow.successUrlDetailView)
  })
})
