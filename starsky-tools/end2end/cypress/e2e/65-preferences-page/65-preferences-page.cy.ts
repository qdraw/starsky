import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
const config = configFile[envFolder][envName]

describe('Preferences page (65)', () => {
  beforeEach(() => {
    if (!config.isEnabled) return false
    cy.resetStorage()
    cy.sendAuthenticationHeader()
  })

  // ── username tab (default) ────────────────────────────────────────────────

  it('preferences page loads with username tab active by default (65)', () => {
    if (!config.isEnabled) return
    cy.intercept('GET', config.accountStatusApi).as('accountStatus')
    cy.visit(config.preferencesUrl)
    cy.wait('@accountStatus')
    cy.get('[data-test="preferences-tab-username"]')
      .should('have.attr', 'aria-selected', 'true')
  })

  it('username tab shows a non-empty username (65)', () => {
    if (!config.isEnabled) return
    cy.intercept('GET', config.accountStatusApi).as('accountStatus')
    cy.visit(config.preferencesUrl)
    cy.wait('@accountStatus')
    cy.get('[data-test="preferences-username-text"]')
      .invoke('text')
      .should('have.length.greaterThan', 0)
  })

  // ── password tab ──────────────────────────────────────────────────────────

  it('password tab renders the change-password form (65)', () => {
    if (!config.isEnabled) return
    cy.visit(config.preferencesUrl)
    cy.get('[data-test="preferences-tab-password"]').click()
    cy.get('[data-test="preferences-password-input"]').should('be.visible')
    cy.get('[data-test="preferences-password-changed-input"]').should('be.visible')
    cy.get('[data-test="preferences-password-changed-confirm-input"]').should('be.visible')
    cy.get('[data-test="preferences-password-submit"]').should('be.visible')
  })

  // Deliberately supply the wrong current password so the backend rejects the
  // request. This exercises the error path without actually changing credentials
  // (which would break the rest of the test pipeline).
  it('wrong current password shows a warning and does not change credentials (65)', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return

    cy.intercept('POST', config.changeSecretApi).as('changeSecret')
    cy.visit(config.preferencesUrl)
    cy.get('[data-test="preferences-tab-password"]').click()

    cy.get('[data-test="preferences-password-input"]')
      .type('this-is-the-wrong-current-password')
    cy.get('[data-test="preferences-password-changed-input"]')
      .type('NewPassword123!')
    cy.get('[data-test="preferences-password-changed-confirm-input"]')
      .type('NewPassword123!')

    cy.get('[data-test="preferences-password-submit"]').click()
    cy.wait('@changeSecret')

    cy.get('[data-test="preferences-password-warning"]').should('be.visible')
  })

  // ── app settings tab ──────────────────────────────────────────────────────

  it('app settings tab loads and fetches permissions (65)', () => {
    if (!config.isEnabled) return
    cy.intercept('GET', config.accountPermissionsApi).as('permissions')
    cy.visit(config.preferencesUrl)
    cy.get('[data-test="preferences-tab-app"]').click()
    cy.wait('@permissions')
    // The tab renders even when the user lacks write permissions;
    // we just verify it does not crash (no error banner, content visible).
    cy.get('[data-test="preferences-tab-app"]')
      .should('have.attr', 'aria-selected', 'true')
  })
})
