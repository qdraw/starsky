import { uploadFileName1 } from 'integration/10-upload-to-folder/upload-filename1'
import { checkIfExistAndCreate } from 'integration/helpers/create-directory-helper'
import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
const config = configFile[envFolder][envName]

describe('Delete file from upload (50)', () => {
  beforeEach('Check some config settings and do them before each test', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()
  })

  const fileName2 = '20200822_111408.jpg'
  const fileName1 = '20200822_112430.jpg'
  const fileName3 = '20200822_134151.jpg'

  it('uploadFileName1 (to make sure the config is right)', () => {
    // clean trash
    cy.request({
      failOnStatusCode: false,
      method: 'DELETE',
      url: '/starsky/api/delete',
      qs: {
        f: `/starsky-end2end-test/${fileName1};/starsky-end2end-test/${fileName2};/starsky-end2end-test/${fileName3};`
      }
    })

    checkIfExistAndCreate(config)
  })

  // Copy
  it('[duplicate] Upload more content and check if the name exist', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return

    uploadFileName1(config.url, fileName1)

    // and from here on its duplicate
    cy.visit(config.url)

    cy.get('.item.item--more').click()
    cy.get('.menu-option--input label').click()

    const fileType = 'image/jpeg'
    const fileInput = '.menu-option--input input[type=file]'

    cy.uploadFile(fileName2, fileType, fileInput)
    cy.wait(1000)

    cy.get('[data-test=upload-files] li').should(($lis) => {
      expect($lis).to.have.length(1)
      expect($lis.eq(0)).to.contain(fileName2)
    })

    cy.uploadFile(fileName3, fileType, fileInput)
    cy.wait(1000)

    cy.get('[data-test=upload-files] li').should(($lis) => {
      expect($lis).to.have.length(1)
      expect($lis.eq(0)).to.contain(fileName3)
    })

    cy.get('.modal-exit-button').click()
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

    cy.wait(3000)
    cy.visit(config.trash)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--more').click()
    cy.get('[data-test=restore-from-trash]').click()

    // item should be in the trash
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).should('not.exist')

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

    cy.wait(3000)
    cy.visit(config.trash)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--more').click()
    cy.get('[data-test=delete]').click()

    // verwijder onmiddelijk
    cy.get('.modal .btn.btn--default').click()

    // item should be in the trash
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).should('not.exist')

    cy.visit(config.url)

    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(2)
    })
  })
})
