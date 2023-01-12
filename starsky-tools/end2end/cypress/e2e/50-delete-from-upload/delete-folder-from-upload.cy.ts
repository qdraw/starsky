import { checkIfExistAndCreate } from '../helpers/create-directory-helper.cy'
import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import { RequestOptions } from 'http'
const config = configFile[envFolder][envName]

describe('Delete folder from upload', () => {
  beforeEach('Check some config settings and do them before each test', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()

    // clean trash
    cy.request({
      failOnStatusCode: false,
      method: 'DELETE',
      url: '/starsky/api/delete',
      qs: {
        f: '/starsky-end2end-test'
      }
    })

    cy.request({
      failOnStatusCode: false,
      method: 'POST',
      url: '/starsky/api/synchronize',
      qs: {
        f: '/starsky-end2end-test'
      }
    })

    // check if folder /starsky-end2end-test is here
    checkIfExistAndCreate(config)
  })

  it('remove folder and clean trash afterwards', () => {
    if (!config.isEnabled) return
    cy.visit(config.urlHome)

    cy.get('.item.item--select').click()
    cy.get('[data-filepath="/starsky-end2end-test"] button').click({ force: true })

    cy.get('.item.item--more').click()
    cy.get('[data-test=trash]').click()

    cy.wait(2000)
    cy.visit(config.trash)

    cy.get('.item.item--select').click()
    // force can be outside of scroll area
    cy.get('[data-filepath="/starsky-end2end-test"] button').click({ force: true })

    cy.get('.item.item--more').click()
    cy.get('[data-test=delete]').click()

    // verwijder onmiddelijk
    cy.intercept('/starsky/api/delete').as('delete_dir')
    cy.get('.modal .btn.btn--default').click()
    cy.wait('@delete_dir')

    // // item should be in the trash
    cy.get('[data-filepath="/starsky-end2end-test"] button').should('not.exist')

    cy.request('POST', config.searchClearCache)
    // and its gone in the api
    cy.request(config.checkIfDirExistApi, {
      failOnStatusCode: false,
      method: 'GET',
      headers: {
        'Content-Type': 'text/plain'
      }
    } as RequestOptions).then((response) => {
      expect(response.status).to.eq(200)
      cy.log(JSON.stringify(response.body))
      expect(response.body.fileIndexItems).to.have.length(0)
    })
  })
})
