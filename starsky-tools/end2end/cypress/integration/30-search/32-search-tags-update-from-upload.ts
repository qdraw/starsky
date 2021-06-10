import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('Search -from upload - update tags', () => {
  beforeEach('Check some config settings and do them before each test', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()

    // clean cache before running
    cy.log(config.searchClearCache)
    cy.request('POST', config.searchClearCache)
    cy.request({
      method: 'POST',
      url: config.clearCacheApi,
      failOnStatusCode: false
    })
  })

  const fileName1 = '20200822_134151.jpg'
  const helloWorldText = 'Hello, World'
  const cleanText = 'ABC'

  const secondAddedText = 'test'

  it('update and overwrite first image', () => {
    if (!config.isEnabled) return

    cy.intercept('/search?t=-inurl:starsky-end2end-test%20-imageformat:jpg').as('search')
    cy.visit(config.urlSearchFromUpload)
    cy.wait('@search')

    // newest first

    cy.get('.folder > div:nth-child(1)').invoke('attr', 'data-filepath')
      .should('equal', `/starsky-end2end-test/${fileName1}`)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--labels').click()

    cy.get('[data-name=tags]').type(helloWorldText)

    cy.get('[data-test=overwrite]').click()

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] .tags`)
      .should('have.text', helloWorldText)
  })

  it('update and overwrite first image after cache clear', () => {
    if (!config.isEnabled) return
    cy.wait(500)

    cy.request('POST', config.searchClearCache)

    cy.intercept('/search?t=-inurl:starsky-end2end-test%20-imageformat:jpg').as('search')
    cy.visit(config.urlSearchFromUpload)
    cy.wait('@search')

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] .tags`)
      .should('contain.text', helloWorldText)
  })

  it('append text to first image', () => {
    if (!config.isEnabled) return

    cy.intercept('/search?t=-inurl:starsky-end2end-test%20-imageformat:jpg').as('search')
    cy.visit(config.urlSearchFromUpload)
    cy.wait('@search')

    cy.get('.folder > div:nth-child(1)').invoke('attr', 'data-filepath')
      .should('equal', `/starsky-end2end-test/${fileName1}`)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--labels').click()

    cy.get('[data-name=tags]').type(secondAddedText)

    cy.get('[data-test=add]').click()

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] .tags`)
      .should('contain.text', ', ' + secondAddedText)
  })

  it('update and add first image after cache clear', () => {
    if (!config.isEnabled) return

    cy.wait(500)
    cy.request('POST', config.searchClearCache)

    cy.intercept('/search?t=-inurl:starsky-end2end-test%20-imageformat:jpg').as('search')
    cy.visit(config.urlSearchFromUpload)
    cy.wait('@search')

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] .tags`)
      .should('contain.text', secondAddedText)
  })

  it('clean text afterwards to something different', () => {
    if (!config.isEnabled) return

    cy.intercept('/search?t=-inurl:starsky-end2end-test%20-imageformat:jpg').as('search')
    cy.visit(config.urlSearchFromUpload)
    cy.wait('@search')

    cy.get('.folder > div:nth-child(1)').invoke('attr', 'data-filepath')
      .should('equal', `/starsky-end2end-test/${fileName1}`)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--labels').click()

    cy.get('[data-name=tags]').type(cleanText)

    cy.get('[data-test=overwrite]').click()

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] .tags`)
      .should('contain.text', cleanText)
  })
})
