import { uploadFileName1 } from 'integration/10-upload-to-folder/upload-filename1'
import { checkIfExistAndCreate } from 'integration/helpers/create-directory-helper'
import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
const config = configFile[envFolder][envName]

describe('Delete file from upload', () => {
  beforeEach('Check some config settings and do them before each test', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()
  })

  const fileName1 = '20200822_111408.jpg'

  it('uploadFileName1 (to make sure the config is right)', () => {
    checkIfExistAndCreate(config)
    uploadFileName1(config.url, fileName1, false)
  })

  it('remove first on to trash and undo afterwards', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--more').click()
    cy.get('[data-test=trash]').click()

    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(2)
    })

    cy.wait(1000)
    cy.visit(config.trash)

    cy.get('.item.item--select').click()
    cy.get('[data-filepath="/starsky-end2end-test/20200822_111408.jpg"] button').click()

    cy.get('.item.item--more').click()
    cy.get('[data-test=restore-from-trash]').click()

    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.class('warning-box')
    })

    cy.visit(config.url)

    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(3)
    })
  })

  it('remove item and remove from trash', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--more').click()
    cy.get('[data-test=trash]').click()

    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(2)
    })

    cy.wait(1000)
    cy.visit(config.trash)

    cy.get('.item.item--select').click()
    cy.get('[data-filepath="/starsky-end2end-test/20200822_111408.jpg"] button').click()

    cy.get('.item.item--more').click()
    cy.get('[data-test=delete]').click()

    // verwijder onmiddelijk
    cy.get('.modal .btn.btn--default').click()

    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.class('warning-box')
    })

    cy.visit(config.url)

    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(2)
    })
  })
})
