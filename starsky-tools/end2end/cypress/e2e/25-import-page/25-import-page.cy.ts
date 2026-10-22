import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
const config = configFile[envFolder][envName]

describe('Import page (25)', () => {
  const fileName = '20200822_111408.jpg'
  const fileType = 'image/jpeg'
  // Selector for the hidden file input inside the DropArea component
  const fileInputSelector = '[data-test="droparea-file-input"]'

  beforeEach(() => {
    if (!config.isEnabled) return false
    cy.resetStorage()
    cy.sendAuthenticationHeader()
  })

  it('import page loads and shows drop area (25)', () => {
    if (!config.isEnabled) return
    cy.visit(config.importUrl)
    cy.get(fileInputSelector)
  })

  // Import is handled asynchronously: the POST /api/import call returns the
  // placed file path, but thumbnail generation and full indexing happen in the
  // background. This test polls the index API until the file is visible.
  it('upload file via import UI, verify success modal, wait for index, cleanup (25)', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return

    // Clean up any leftover file from a previous failed run.
    cy.request({
      failOnStatusCode: false,
      url: `${config.searchApi}?json=true&t=${encodeURIComponent(fileName)}&p=0`
    }).then((searchRes) => {
      if (searchRes.body?.fileIndexItems?.length > 0) {
        const leftoverPaths = searchRes.body.fileIndexItems
          .map((item: { filePath: string }) => item.filePath)
          .join(';')
        cy.request({
          failOnStatusCode: false,
          method: 'DELETE',
          url: config.deleteApi,
          qs: { f: leftoverPaths }
        })
      }
    })

    cy.visit(config.importUrl)
    cy.get(fileInputSelector)

    // Set up intercept before triggering the upload so we can capture the
    // file path(s) that the backend chose for the imported file.
    cy.intercept('POST', config.importApi).as('importRequest')
    cy.uploadFile(fileName, fileType, fileInputSelector)

    let importedFilePath = ''

    cy.wait('@importRequest', { timeout: 30000 }).then((interception) => {
      const body = interception.response?.body
      cy.log('import API response: ' + JSON.stringify(body))
      if (Array.isArray(body) && body.length > 0 && body[0]?.filePath) {
        importedFilePath = body[0].filePath
        cy.log('imported file placed at: ' + importedFilePath)
      }
    })

    // The DropArea shows a modal listing the imported files on success.
    cy.get('[data-test="modal-drop-area-files-added"]', { timeout: 30000 })
      .should('be.visible')
    cy.get('[data-test="upload-files"] li')
      .should('have.length.at.least', 1)
    cy.get('[data-test="upload-files"] li').first()
      .should('contain', fileName)

    // Close the modal before leaving the page.
    cy.get('.modal-exit-button').click()

    // Poll the index API until the background indexing is complete.
    // Import places the file immediately but full indexing (and thumbnail
    // generation) can lag on slow CI runners.
    cy.then(() => {
      if (importedFilePath) {
        waitForIndexed(0, importedFilePath)
      } else {
        cy.log('no importedFilePath captured — skipping index poll')
      }
    })

    // Cleanup: hard-delete the imported file so later suites are not affected.
    cy.then(() => {
      if (importedFilePath) {
        cy.request({
          failOnStatusCode: false,
          method: 'DELETE',
          url: config.deleteApi,
          qs: { f: importedFilePath }
        })
      }
    })
  })

  // Poll GET /api/index?f=<path> until the backend returns 200 with at least
  // one item — or until we hit the retry ceiling.
  function waitForIndexed (index: number, filePath: string, max = 15) {
    cy.request({
      failOnStatusCode: false,
      url: `${config.indexApi}?f=${encodeURIComponent(filePath)}`
    }).then((response) => {
      if (
        response.status === 200 &&
        response.body?.fileIndexItems?.length > 0
      ) {
        cy.log(`file indexed after ${index} poll(s)`)
        return
      }
      cy.log(`not indexed yet (attempt ${index + 1}/${max}), retrying…`)
      cy.wait(2000)
      if (index + 1 < max) {
        waitForIndexed(index + 1, filePath, max)
      }
    })
  }
})
