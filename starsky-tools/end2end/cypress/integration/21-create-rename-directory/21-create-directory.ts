import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import { checkIfExistAndCreate } from '../helpers/create-directory-helper'
const config = configFile[envFolder][envName]

/**
 * WORK
 *       IN
 *          PROGRESS
 *                  NOT COMPLETE
 */
describe('Create Rename Dir', () => {
  beforeEach('Check some config settings and do them before each test', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()
  })

  it('Check if folder is there & create', () => {
    if (!config.isEnabled) return
    checkIfExistAndCreate(config)
  })

  it('Create new folder', () => {
    if (!config.isEnabled) return

    cy.visit(config.url)

    cy.get('.item.item--more').click()
    cy.get('[data-test=mkdir]').click()

    // dont include in end2end-test
    cy.get('[data-name=directoryname]').type('z_test_auto_created')
    cy.get('.btn.btn--default').click()

    cy.request(config.urlMkdir + '/z_test_auto_created')
  })

  it('Rename new folder', () => {
    if (!config.isEnabled) return

    cy.visit(config.url + '/z_test_auto_created')

    cy.get('.item.item--more').click()
    cy.get('[data-test=rename]').click()

    cy.get('[data-name=foldername]').type('_update')
    cy.get('.btn.btn--default').click()

    cy.request(config.urlMkdir + '/z_test_auto_created_update')
  })
})
