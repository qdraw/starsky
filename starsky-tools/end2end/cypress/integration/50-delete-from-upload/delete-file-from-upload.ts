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
  const fileName4 = '20200822_134151.mp4'

  it('clear cache & upload all files that are needed in background', () => {
    // clean trash
    cy.request({
      failOnStatusCode: false,
      method: 'DELETE',
      url: '/starsky/api/delete',
      qs: {
        f: `/starsky-end2end-test/${fileName1};/starsky-end2end-test/${fileName2};/starsky-end2end-test/${fileName3};/starsky-end2end-test/${fileName4}`
      }
    })

    checkIfExistAndCreate(config)

    cy.fileRequest(
      fileName4,
      '/starsky-end2end-test',
      'image/jpeg'
    )
    cy.fileRequest(
      fileName1,
      '/starsky-end2end-test',
      'image/jpeg'
    )
    cy.fileRequest(
      fileName2,
      '/starsky-end2end-test',
      'image/jpeg'
    )
    cy.fileRequest(
      fileName3,
      '/starsky-end2end-test',
      'image/jpeg'
    )

    cy.wait(500)

    cy.request(config.urlApiCollectionsFalse).then((res) => {
      expect(res.status).to.eq(200)
      expect(res.body.fileIndexItems.length).to.eq(4)
    })
  })

  it('remove collection item, but not the other file', () => {
    cy.visit(config.urlVideoItemCollectionsFalse)

    cy.get('.item.item--more').click()
    cy.get('[data-test=trash]').click()

    cy.visit(config.url)
    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(3)
    })
    cy.wait(4000)
    cy.visit(config.trash)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName4}"] button`).click()

    cy.request(config.urlApiCollectionsFalse).then((res) => {
      expect(res.status).to.eq(200)
      expect(res.body.fileIndexItems.length).to.eq(3)
    })
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
