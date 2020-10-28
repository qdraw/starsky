import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('Search', () => {
  beforeEach('Check some config settings and do them before each test', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()
  })

  it('Type and go to search page', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)

    cy.get(flow.fields.search).type(flow.fields.searchData)
      .url()
      .should('contain', flow.successUrl)

    cy.get(flow.boxContent)
  })

  it('Go Direct to search page', () => {
    if (!config.isEnabled) return
    cy.visit(config.urlSearchJpg)
    cy.get(flow.boxContent)
  })

  it('Navigate to first detailview item', () => {
    if (!config.isEnabled) return
    cy.visit(config.urlSearchJpg)
    cy.get(flow.boxContent).first().click()
      .url()
      .should('contain', flow.successUrlDetailView)
  })
})
