import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('Archive (from upload)', () => {
  beforeEach('Check some config settings and do them before each test', () => {
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

  it('Check if folder is there', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)
    cy.get(flow.content)
  })

  it('should match order', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)

    cy.get('.folder > div:nth-child(1)').invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_111408.jpg')

    cy.get('.folder > div:nth-child(2)').invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_112430.jpg')

    cy.get('.folder > div:nth-child(3)').invoke('attr', 'data-filepath')
      .should('equal', '/starsky-end2end-test/20200822_134151.jpg')
  })

  it('contains elements', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)

    cy.get('[data-filepath="/starsky-end2end-test/20200822_111408.jpg"]').should('have.length', 1)
    cy.get('[data-filepath="/starsky-end2end-test/20200822_112430.jpg"]').should('have.length', 1)
    cy.get('[data-filepath="/starsky-end2end-test/20200822_134151.jpg"]').should('have.length', 1)
  })

  it('test realtime update', () => {
    if (!config.isEnabled) return

    cy.visit(config.url)

    cy.request({
      url: (config.indexApi as string) + '?f=' + '/starsky-end2end-test/20200822_111408.jpg'
    }).then((response) => {
      const sourceTags = response.body.fileIndexItem.tags

      cy.log('next step: background update')

      cy.request({
        method: 'POST',
        url: config.updateApi, // baseUrl is prepend to URL
        form: true, // indicates the body should be form urlencoded and sets Content-Type: application/x-www-form-urlencoded headers
        body: {
          append: false,
          collections: true,
          tags: 'realtime-update-test',
          f: '/starsky-end2end-test/20200822_111408.jpg'
        }
      })

      cy.request({
        url: (config.indexApi as string) + '?f=' + '/starsky-end2end-test/20200822_111408.jpg'
      }).then((response2) => {
        cy.log('next step: check api')

        const sourceTags2 = response2.body.fileIndexItem.tags
        expect(sourceTags2).eq('realtime-update-test')

        cy.request({
          url: (config.notificationApi as string)
        }).then((response3) => {
          const notification = response3.body as [{ content: string }]

          const dataObjects: any[] = []
          for (const item1 of notification) {
            if (item1.content.includes('realtime-update-test') &&
              item1.content.includes('/starsky-end2end-test/20200822_111408.jpg') &&
              item1.content.includes('"type":"MetaUpdate"')) {
              cy.log('next step: check notification')
              dataObjects.push(item1.content)
            }
          }

          if (dataObjects.length === 0) {
            resetAfterwards(sourceTags)
            throw new Error('No realtime update found')
          }

          cy.reload()

          cy.log('next step: check DOM')

          cy.get('[data-filepath="/starsky-end2end-test/20200822_111408.jpg"]').should('have.length', 1)

          cy.get('[data-filepath="/starsky-end2end-test/20200822_111408.jpg"] .tags').should('have.length', 1)

          try {
            cy.get('[data-filepath="/starsky-end2end-test/20200822_111408.jpg"] .tags').contains('test')
          } catch (error) {
            resetAfterwards(sourceTags)
            throw error
          }
          resetAfterwards(sourceTags)
        })
      })
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
