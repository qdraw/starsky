import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
const config = configFile[envFolder][envName]

describe('Search -from upload - update tags (32)', () => {
  const searchRoute = '**/search?t=*inurl:starsky-end2end-test*imageformat:jpg*'
  const updateRoute = '**/api/update*'

  const postNoFail = (url: string) => cy.request({
    method: 'POST',
    url,
    failOnStatusCode: false
  })

  const clearSearchCaches = () => {
    postNoFail(config.searchClearCache)
    postNoFail('/starsky' + config.searchClearCache)
    postNoFail(config.clearCacheApi)
    postNoFail('/starsky' + config.clearCacheApi)
  }

  const visitAndWaitForSearch = () => {
    cy.intercept(searchRoute).as('search')
    cy.visit(config.urlSearchFromUpload)
    cy.wait('@search')
  }

  beforeEach('Check some config settings and do them before each test (32)', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()

    // clean cache before running
    clearSearchCaches()
  })

  const fileName1 = '20200822_134151.jpg'
  const helloWorldText = 'Hello, World'
  const cleanText = 'ABC'

  const secondAddedText = 'test'

  it('update and overwrite first image (32)', () => {
    if (!config.isEnabled) return

    visitAndWaitForSearch()

    // newest first

    cy.get('.folder > div:nth-child(1)').invoke('attr', 'data-filepath')
      .should('equal', `/starsky-end2end-test/${fileName1}`)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--labels').click()

    cy.get('[data-name=tags]').clear().type(helloWorldText)

    cy.intercept(updateRoute).as('update')
    cy.get('[data-test=overwrite]').click()
    cy.wait('@update')

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] .tags`)
      .should('contain.text', helloWorldText)
  })

  it('update and overwrite first image after cache clear (32)',
    {
      retries: 4
    }, () => {
      if (!config.isEnabled) return

      clearSearchCaches()
      // need to wait for backend
      cy.wait(1000)

      clearSearchCaches()

      cy.wait(1000)

      visitAndWaitForSearch()

      cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] .tags`)
        .should('contain.text', helloWorldText)
    })

  it('append text to first image (32)', () => {
    if (!config.isEnabled) return

    visitAndWaitForSearch()

    cy.get('.folder > div:nth-child(1)').invoke('attr', 'data-filepath')
      .should('equal', `/starsky-end2end-test/${fileName1}`)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--labels').click()

    cy.get('[data-name=tags]').clear().type(secondAddedText)

    cy.intercept(updateRoute).as('update')
    cy.get('[data-test=add]').click()
    cy.wait('@update')

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] .tags`)
      .should('contain.text', ', ' + secondAddedText)
  })

  it('update and add first image after cache clear (32)',
    {
      retries: 4
    }, () => {
      if (!config.isEnabled) return

      // need to wait for backend
      cy.wait(1000)

      clearSearchCaches()

      cy.wait(1000)

      visitAndWaitForSearch()

      cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] .tags`)
        .should('contain.text', secondAddedText)
    })

  it('clean text afterwards to something different (32)', () => {
    if (!config.isEnabled) return

    visitAndWaitForSearch()

    cy.get('.folder > div:nth-child(1)').invoke('attr', 'data-filepath')
      .should('equal', `/starsky-end2end-test/${fileName1}`)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--labels').click()

    cy.get('[data-name=tags]').clear().type(cleanText)

    cy.intercept(updateRoute).as('update')
    cy.get('[data-test=overwrite]').click()
    cy.wait('@update')

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] .tags`)
      .should('contain.text', cleanText)
  })
})
