import { RequestOptions } from 'http'
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

  it('Check if folder is there', () => {
    // if (!config.isEnabled) return
    // te
  })
})
