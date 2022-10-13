import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('Logout', () => {
  beforeEach('Check some config settings and do them before each test', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()
  })

  it('logout page is here', () => {
    if (!config.isEnabled) return false
    cy.checkStatusCode(config.url)
  })

  it('does logout into app', { }, () => {
    if (!config.isEnabled) return false

    /* Start flow (connection header prevents script from
        occassionaly throwing ESOCKETTIMEDOUT errors in CI) */
    cy.visit(config.url)

    cy.get(flow.logout)
      .click()
      .url()
      .should('contain', config.successUrl)

    cy.get(flow.form)

    // check api if your log outed
    cy.request({
      url: config.statusApi,
      failOnStatusCode: false
    }).then((res) => {
      expect(res.status).to.eq(401)
    })
  })
})
