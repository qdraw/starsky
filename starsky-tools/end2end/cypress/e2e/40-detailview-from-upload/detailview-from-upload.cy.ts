import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('DetailView (from upload) (40)', () => {
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

  const fileName2 = '20200822_111408.jpg'
  const fileName1 = '20200822_112430.jpg'
  const fileName3 = '20200822_134151.jpg'

  it('Check if folder is there and if files are in folder (40)', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)
    cy.get(flow.content)
    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(3)
      expect($lis.eq(0)).to.contain(fileName2)
      expect($lis.eq(1)).to.contain(fileName1)
      expect($lis.eq(2)).to.contain(fileName3)
    })
  })

  it('Click on first item (40)', () => {
    if (!config.isEnabled) return
    cy.visit(config.url)
    cy.get(flow.boxContent).first().click()
      .url()
      .should('contain', fileName2)
  })

  it('Click on close (40)', () => {
    if (!config.isEnabled) return
    cy.visit(config.url + '/' + fileName2)
    cy.get('.item.item--first.item--close').click()
    cy.get(flow.content)
  })

  it('go next to filename3 (40)', () => {
    if (!config.isEnabled) return
    cy.visit(config.url + '/' + fileName2)

    cy.intercept('/starsky/api/index?f=/starsky-end2end-test/20200822_112430.jpg').as('index1')

    cy.get('.nextprev.nextprev--next').first()
      .click()

    cy.wait('@index1')

    cy.url()
      .should('contain', fileName1)

    cy.get('.nextprev.nextprev--next').first().click()

    cy.url()
      .should('contain', fileName3)
  })

  it('go back to fileName2 (40)', {
    retries: { runMode: 3, openMode: 3 }
  }, () => {
    if (!config.isEnabled) return

    cy.clearLocalStorage()
    cy.visit(`${config.url}/${fileName3}`)

    cy.intercept('/starsky/api/index?f=/starsky-end2end-test/20200822_112430.jpg').as('index1')

    cy.get('.nextprev.nextprev--prev').first().click()
      .url()
      .should('contain', fileName1)

    cy.wait('@index1')
    cy.wait(100)

    cy.get('.nextprev.nextprev--prev').first().click()
      .url()
      .should('contain', fileName2)
  })

  it('update label data (40)', () => {
    if (!config.isEnabled) return
    cy.visit(config.url + '/' + fileName1)

    cy.get('.item.item--labels').click()

    // sourceTags is secretly updated during the run
    let sourceTags = ''

    const tagFileSelector = "[data-name='tags']"
    const appendText = ', test'

    cy.get(tagFileSelector).then(elems => {
      sourceTags = elems.text()
      cy.log(sourceTags)
    }).then(() => {
      cy.get(tagFileSelector).type(appendText)
      cy.get(tagFileSelector).blur()
    }).then(() => {
      sessionStorage.clear()
      cy.reload()
    }).then(() => {
      cy.get(tagFileSelector).should('contain', 'test')
      // sourceTags does now contain test? #wtf
      cy.get(tagFileSelector).should('contain', sourceTags)
    }).then(() => {
      // and now we going back to the orginal state
      cy.get(tagFileSelector).type('{backspace}'.repeat(appendText.length))
      cy.get(tagFileSelector).blur()
    }).then(() => {
      sessionStorage.clear()
      cy.reload()
    }).then(() => {
      cy.get(tagFileSelector).should('not.contain', 'test')
    })
  })
})
