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
    cy.wait(500)

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
    waitOnUploadIsDone(0)
  })

  it('check if upload is done', () => {
    cy.request(config.urlApiCollectionsFalse).then((res) => {
      expect(res.status).to.eq(200)
      expect(res.body.fileIndexItems.length).to.eq(4)
    })
  })

  function waitOnUploadIsDone (index:number, max: number = 10) {
    cy.request({
      url: config.urlApiCollectionsFalse,
      method: 'GET',
      headers: {
        'Content-Type': 'text/plain'
      }
    }).then((response) => {
      expect(response.status).to.eq(200)
      cy.log(JSON.stringify(response.body.fileIndexItems))

      if (response.body.fileIndexItems.length === 4) {
        cy.log('4 items, done')
        return
      }
      cy.wait(1500)
      index++
      if (index < max) {
        waitOnUploadIsDone(index, max)
      }
    })
  }

  it('remove collection item, but not the other file', () => {
    cy.visit(config.urlVideoItemCollectionsFalse)

    cy.get('.item.item--more').click()
    cy.get('[data-test=trash]').click()

    cy.visit(config.url)
    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(3)
    })

    waitFileInTrash(0, `/starsky-end2end-test/${fileName4}`)

    cy.log(`go to: ${config.trash}`)
    cy.visit(config.trash)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName4}"] button`).click()

    // more menu and delete
    cy.get('.item.item--more').click()
    cy.get('[data-test=delete]').click()

    // verwijder onmiddelijk
    cy.intercept('/starsky/api/delete').as('delete4')
    cy.get('.modal .btn.btn--default').click()
    cy.wait('@delete4')

    cy.request(config.urlApiCollectionsFalse).then((res) => {
      expect(res.status).to.eq(200)
      expect(res.body.fileIndexItems.length).to.eq(3)
    })

    cy.visit(config.url)

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName3}"]`)

    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(3)
    })
  })

  function waitFileInTrash (index:number, filePath: string, max: number = 10) {
    cy.request({
      url: '/api/search/trash',
      method: 'GET',
      headers: {
        'Content-Type': 'text/plain'
      }
    }).then((response) => {
      expect(response.status).to.eq(200)
      cy.log(JSON.stringify(response.body.fileIndexItems))

      for (const item of response.body.fileIndexItems) {
        if (item.filePath === filePath) {
          cy.log('in trash end')
          return
        }
      }
      cy.wait(1500)
      index++
      if (index < max) {
        waitFileInTrash(index, filePath, max)
      }
    })
  }

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

    waitFileInTrash(0, `/starsky-end2end-test/${fileName1}`)
    cy.visit(config.trash)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    // restore
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

    waitFileInTrash(0, `/starsky-end2end-test/${fileName1}`)
    cy.visit(config.trash)

    cy.get('.item.item--select').click()
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).click()

    cy.get('.item.item--more').click()
    cy.get('[data-test=delete]').click()

    // verwijder onmiddelijk
    cy.intercept('/starsky/api/delete').as('delete1')
    cy.get('.modal .btn.btn--default').click()
    cy.wait('@delete1')

    // item should be in the trash
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`).should('not.exist')

    cy.visit(config.url)

    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(2)
    })
  })
})
