import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import flow from './flow.json'
const config = configFile[envFolder][envName]

describe('Create Account (01/02)', () => {
  it('check if TOC page exist', () => {
    if (!config.isEnabled) return false

    cy.visit(config.url)

    cy.get(flow.toc.tocAhref).click()
      .url()
      .should('contain', flow.toc.tocUrl)
  })

  it('check if privacy page exist (01/02)', () => {
    if (!config.isEnabled) return false

    cy.visit(config.url)

    cy.get(flow.toc.privacyAhref).click()
      .url()
      .should('contain', flow.toc.privacyUrl)
  })
})
