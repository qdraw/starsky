import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]
import { checkIfExistAndCreate, waitOnUploadIsDone } from "../helpers/create-directory-helper.cy";

describe('Archive (from upload) (21)', () => {
  beforeEach('Check some config settings and do them before each test (21)', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()

    // check if folder /starsky-end2end-test is here
    cy.request({
      url: config.urlMkdir,
      failOnStatusCode: true,
      method: 'GET',
      headers: {
        'Content-Type': 'text/plain'
      }
    })
  })

    const fileName2 = "20200822_111408.jpg";
    const fileName1 = "20200822_112430.jpg";
    const fileName3 = "20200822_134151.jpg";
    const fileName4 = "20200822_134151.mp4";
  
    it("clear cache & upload all files that are needed in background (21)", () => {
      // clean trash
      cy.request({
        failOnStatusCode: false,
        method: "DELETE",
        url: "/starsky/api/delete",
        qs: {
          f: `/starsky-end2end-test/${fileName1};/starsky-end2end-test/${fileName2};/starsky-end2end-test/${fileName3};/starsky-end2end-test/${fileName4}`,
        },
      });
      cy.wait(500);
  
      checkIfExistAndCreate(config);
  
      cy.fileRequest(fileName4, "/starsky-end2end-test", "image/jpeg");
      cy.fileRequest(fileName1, "/starsky-end2end-test", "image/jpeg");
      cy.fileRequest(fileName2, "/starsky-end2end-test", "image/jpeg");
      cy.fileRequest(fileName3, "/starsky-end2end-test", "image/jpeg");
      waitOnUploadIsDone(config.urlApiCollectionsFalse, 0);
    });

  it('Check if folder is there (21)', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)
    cy.get(flow.content)
  })

  it('should match order (21)', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)

    cy.get('.folder > div:nth-child(1)')
      .invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_111408.jpg')

    cy.get('.folder > div:nth-child(2)')
      .invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_112430.jpg')

    cy.get('.folder > div:nth-child(3)')
      .invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_134151.jpg')
  })

  it('contains elements (21)', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)

    cy.get(
      '[data-filepath="/starsky-end2end-test/20200822_111408.jpg"]'
    ).should('have.length', 1)
    cy.get(
      '[data-filepath="/starsky-end2end-test/20200822_112430.jpg"]'
    ).should('have.length', 1)
    cy.get(
      '[data-filepath="/starsky-end2end-test/20200822_134151.jpg"]'
    ).should('have.length', 1)
  })

  it('test realtime update notification api 3 (21)', () => {
    if (!config.isEnabled) return

    const sourceTags = 'tete de balacha, bergtop, mist, flaine'
    const keyword = `realtime-update-test${Math.floor(Math.random() * 100)}`

    cy.sendAuthenticationHeader()
    cy.request({
      method: 'POST',
      url: config.updateApi, // baseUrl is prepend to URL
      form: true, // indicates the body should be form urlencoded and sets Content-Type: application/x-www-form-urlencoded headers
      body: {
        append: false,
        collections: true,
        tags: keyword,
        f: '/starsky-end2end-test/20200822_111408.jpg'
      }
    })

    cy.request({
      url: config.notificationApi as string
    }).then((response3) => {
      const notification = response3.body as [{ content: string }]
      cy.log(notification[0].content)

      const dataObjects: string[] = []
      for (const item1 of notification) {
        if (
          item1.content.includes(keyword) &&
					item1.content.includes('/starsky-end2end-test/20200822_111408.jpg') &&
					item1.content.includes('"type":"MetaUpdate"')
        ) {
          dataObjects.push(item1.content)
          cy.log('found notification: ' + item1.content)
        }
      }
      cy.log('next step: check notification')

      if (dataObjects.length === 0) {
        resetAfterwards(sourceTags)
        throw new Error('No realtime update found')
      }

      resetAfterwards(sourceTags)
    })

    resetAfterwards(sourceTags)
  })

  it('test realtime update 2 websockets (21)', {
    retries: { runMode: 4, openMode: 2 }
  }, () => {
    return new Cypress.Promise((resolve) => {

      // skip this test currently for windows
      
      if (!config.isEnabled || Cypress.platform === 'win32') {
        cy.log(`test skipped isEnabled: ${config.isEnabled} platform: ${Cypress.platform}` )
        resolve();
        return;
      }

      const sourceTags = 'tete de balacha, bergtop, mist, flaine'
      const url = Cypress.config().baseUrl
      const socketUrl =
				url
				  .replace(config.url, '')
				  .replace('http://', 'ws://')
				  .replace('https://', 'wss://') + config.realtime
      const socket = new WebSocket(socketUrl)
      const keyword = `realtime-update-test${Math.floor(Math.random() * 100)}`

      socket.onmessage = function (message: {data: string}) {
        if (
          message.data.includes(keyword) &&
					message.data.includes('/starsky-end2end-test/20200822_111408.jpg') &&
					message.data.includes('"type":"MetaUpdate"')
        ) {
          resolve()
        }
      }

      cy.request({
        method: 'POST',
        url: config.updateApi, // baseUrl is prepend to URL
        form: true, // indicates the body should be form urlencoded and sets Content-Type: application/x-www-form-urlencoded headers
        body: {
          append: false,
          collections: true,
          tags: keyword,
          f: '/starsky-end2end-test/20200822_111408.jpg'
        }
      })
      resetAfterwards(sourceTags)
    })
  })

  function resetAfterwards (sourceTags: string): void {
    cy.log('next step: reset afterwards')
    cy.request({
      method: 'POST',
      url: config.updateApi, // baseUrl is prepend to URL
      form: true, // indicates the body should be form urlencoded and sets Content-Type: application/x-www-form-urlencoded headers
      body: {
        append: false,
        collections: true,
        tags: sourceTags,
        f: '/starsky-end2end-test/20200822_111408.jpg'
      }
    })
  }
})
