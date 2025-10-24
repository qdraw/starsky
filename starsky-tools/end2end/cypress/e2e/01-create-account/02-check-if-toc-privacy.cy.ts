import flow from './flow.json'
import config from '../../fixtures/urls.json'
import { resetStorage } from 'support/commands'

describe('Create Account (01/02)', () => {
  it('check if TOC page exist', () => {

    resetStorage()
    cy.visit(config.urlAccountRegister)

    cy.get(flow.toc.tocAhref).click()
      .url()
      .should('contain', flow.toc.tocUrl)
  })

  it('check if privacy page exist (01/02)', () => {

    cy.visit(config.urlAccountRegister)

    cy.get(flow.toc.privacyAhref).click()
      .url()
      .should('contain', flow.toc.privacyUrl)
  })
})
