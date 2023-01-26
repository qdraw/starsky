import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import { checkIfExistAndCreate } from '../helpers/create-directory-helper.cy'
const config = configFile[envFolder][envName]

function resetFolders () {
  cy.request({
    failOnStatusCode: false,
    method: 'POST',
    url: '/starsky/api/update',
    qs: {
      f: '/starsky-end2end-test/z_test_auto_created_update;/starsky-end2end-test/z_test_auto_created',
      tags: '!delete!'
    }
  })

  cy.wait(1000)

  cy.request({
    failOnStatusCode: false,
    method: 'DELETE',
    url: '/starsky/api/delete',
    qs: {
      f: '/starsky-end2end-test/z_test_auto_created_update;/starsky-end2end-test/z_test_auto_created'
    }
  })

  cy.request({
    failOnStatusCode: false,
    url: '/starsky/api/remove-cache?json=true&f=/starsky-end2end-test'
  })
}

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

  it('Create Rename Dir - Check if folder is there & create', () => {
    if (!config.isEnabled) return
    checkIfExistAndCreate(config)
    resetFolders()
  })

  it('Create new folder', () => {
    if (!config.isEnabled) return

    cy.visit(config.url)

    cy.get('.item.item--more').click()
    cy.get('[data-test=mkdir]').click()

    cy.intercept('/starsky/api/disk/mkdir', (req) => {
      req.headers['content-type'] = 'application/x-www-form-urlencoded'
    }).as('mkdir')
    cy.get('[data-name=directoryname]').type('z_test_auto_created')
    cy.get('[data-test=modal-archive-mkdir-btn-default]').click()
    cy.wait('@mkdir')

    cy.visit(config.url)
    cy.get('[data-filepath="/starsky-end2end-test/z_test_auto_created"]').should('exist')
  })

  it('Rename new folder', () => {
    if (!config.isEnabled) return

    cy.visit(config.url + '/z_test_auto_created')

    cy.get('.item.item--more').click()
    cy.get('[data-test=rename]').click()

    cy.intercept(config.apiRename, (req) => {
      req.headers['content-type'] = 'application/x-www-form-urlencoded'
    }).as('rename')

    cy.get('[data-name=foldername]').type('_update')
    cy.get('.btn.btn--default').click()

    cy
      .get('.modal .warning-box')
      .should('not.exist')

    cy.wait('@rename')
    cy.request(config.urlMkdir + '/z_test_auto_created_update')

    cy
      .get('.folder')
      .should('be.visible')

    cy.wait(500)
    cy.visit(config.url)

    cy.get('[data-filepath="/starsky-end2end-test/z_test_auto_created_update"]').should('exist')
    cy.get('[data-filepath="/starsky-end2end-test/z_test_auto_created"]').should('not.exist')
  })

  it('delete it afterwards', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return

    // make sure the folder is there
    cy.request({
      method: 'POST',
      url: config.apiMkdir,
      form: true,
      body: {
        f: '/starsky-end2end-test/z_test_auto_created_update'
      },
      failOnStatusCode: false
    }).then(() => {
      cy.resetStorage()

      cy.visit(config.url)

      cy.get('.item.item--select').click()
      cy.get('[data-filepath="/starsky-end2end-test/z_test_auto_created_update"] button').click()

      cy.get('.item.item--more').click()
      cy.wait(10)

      cy.intercept(config.apiUpdate).as('updateToTrash')

      cy.intercept(config.apiUpdate, (req) => {
        req.headers['content-type'] = 'application/x-www-form-urlencoded'
      }).as('updateToTrash')

      cy.get('[data-test=trash]').click()
      cy.wait('@updateToTrash')

      console.log('next check if is in trash')
      cy.wait(10)

      cy.request(config.apiTrash).then((response) => {
        const message = response.body.fileIndexItems.find((x: any) => x.filePath === '/starsky-end2end-test/z_test_auto_created_update')
        if (message?.filePath === '/starsky-end2end-test/z_test_auto_created_update') {
          cy.log('found')
          cy.log(message)
        } else {
          cy.log('NOT found')
          cy.log(response.body)
          resetFolders()
          throw new Error('not found')
        }

        cy.visit(config.trash)
        cy.get('.item.item--select').click()
        cy.get('[data-filepath="/starsky-end2end-test/z_test_auto_created_update"] button').click({ force: true })

        // menu ->
        cy.get('.item.item--more').click()
        cy.get('[data-test=delete]').click()

        // verwijder onmiddelijk
        cy.get('.modal .btn.btn--default').click()

        // item should be in the trash
        cy.get('[data-filepath="/starsky-end2end-test/z_test_auto_created_update"] button').should('not.exist')

        // and not in the source folder
        cy.visit(config.url)
        cy.get('[data-filepath="/starsky-end2end-test/z_test_auto_created_update"] button').should('not.exist')
        resetFolders()
      })
    })
  })

  it('safe guard for other tests - if not deleted remove via the api', () => {
    if (!config.isEnabled) return

    resetFolders()

    cy.wait(1000)

    cy.intercept(config.url).as('url')

    cy.visit(config.url)

    cy.wait('@url')

    // need to wait until the page is loaded
    cy
      .get('.folder')
      .should('be.visible')

    cy.get('[data-filepath="/starsky-end2end-test/z_test_auto_created_update"]').should('not.exist')
    cy.get('[data-filepath="/starsky-end2end-test/z_test_auto_created"]').should('not.exist')
  })
})
