import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
const config = configFile[envFolder][envName]

// Tests EXIF metadata fields that suite 40 does NOT cover (40 only touches tags).
// Runs after 40 (files already uploaded by suite 10) and before 50 (which deletes them).
describe('DetailView metadata editing (45)', () => {
  // Use the second fixture file so tag state from suite 40 (which edits file 2)
  // and this suite do not step on each other.
  const fileName = '20200822_134151.jpg'
  const detailUrl = `/?f=/starsky-end2end-test/${fileName}`

  beforeEach(() => {
    if (!config.isEnabled) return false
    cy.resetStorage()
    cy.sendAuthenticationHeader()
    // Confirm the test folder still exists before each test.
    cy.request({
      url: config.urlMkdir,
      failOnStatusCode: true,
      method: 'GET',
      headers: { 'Content-Type': 'text/plain' }
    })
  })

  // ── helpers ──────────────────────────────────────────────────────────────

  // Open the file in detail view with the labels sidebar visible.
  function openWithSidebar () {
    cy.visit(detailUrl)
    cy.get('.item.item--labels').should('be.visible').click()
    cy.get('[data-test="detailview-sidebar"]').should('be.visible')
  }

  // Generic helper: edit a contenteditable metadata field, wait for the
  // backend update to confirm, reload to verify persistence, then restore.
  // The intercept is set up AFTER typing so that keystrokes during editing
  // cannot accidentally satisfy the alias early.
  function editFieldAndRestore (
    selector: string,
    newValue: string,
    aliasName: string
  ) {
    let originalValue = ''

    cy.get(selector).then((el) => {
      originalValue = el.text().trim()
      cy.log(`${selector} original value: "${originalValue}"`)
    })

    cy.get(selector).focus()
    cy.get(selector)
      .type('{selectall}')
      .type(newValue, { parseSpecialCharSequences: false })

    cy.intercept('POST', '**/api/update').as(aliasName)
    cy.get(selector).blur()
    cy.wait(`@${aliasName}`)

    // A slow CI backend may still be writing; sessionStorage.clear() + reload
    // bypasses the client-side cache and forces a fresh fetch.
    // The URL still carries ?details=true from openWithSidebar(), so the sidebar
    // is already open after reload — clicking the toggle would close it.
    cy.then(() => { sessionStorage.clear() })
    cy.reload()
    cy.get('.item.item--labels').should('be.visible')
    cy.get('[data-test="detailview-sidebar"]').should('be.visible')
    cy.get(selector).should('contain', newValue)

    // Restore original value.
    cy.get(selector).focus()
    cy.get(selector)
      .type('{selectall}')
      .type(originalValue.length > 0 ? originalValue : '{del}', {
        parseSpecialCharSequences: false
      })
    cy.intercept('POST', '**/api/update').as(`${aliasName}Restore`)
    cy.get(selector).blur()
    cy.wait(`@${aliasName}Restore`)

    cy.then(() => { sessionStorage.clear() })
    cy.reload()
    cy.get('.item.item--labels').should('be.visible')
    cy.get('[data-test="detailview-sidebar"]').should('be.visible')
    cy.get(selector).should('not.contain', newValue)
  }

  // ── tests ─────────────────────────────────────────────────────────────────

  it('detail view opens and sidebar is visible (45)', () => {
    if (!config.isEnabled) return
    openWithSidebar()
  })

  // Description is a free-text EXIF field written to the file on blur.
  // The backend update and thumbnail re-generation are async — the intercept
  // on POST /api/update is enough to confirm the write was accepted before
  // we reload to verify persistence.
  it('edit description field, verify persistence, restore (45)', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return
    openWithSidebar()
    editFieldAndRestore('[data-name="description"]', 'e2e-test-description', 'updateDescription')
  })

  it('edit title field, verify persistence, restore (45)', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return
    openWithSidebar()
    editFieldAndRestore('[data-name="title"]', 'e2e-test-title', 'updateTitle')
  })

  // The datetime modal has its own submit flow rather than a blur.
  // We read the current year, increment it, save, verify, then restore.
  // retries=3 because the modal submit → reload → verify cycle is the most
  // susceptible step to slow async writes on CI.
  it('edit datetime via modal, verify persistence, restore (45)', {
    retries: { runMode: 3, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return
    openWithSidebar()

    const yearSelector = '[data-test="modal-edit-datetime"] [data-name="year"]'
    let originalYear = ''

    cy.get('[data-test="dateTime"]').click()
    cy.get('[data-test="modal-edit-datetime"]').should('be.visible')
    // Capture current year before touching anything.
    cy.get(yearSelector).then((el) => {
      originalYear = el.text().trim()
      cy.log('original year: ' + originalYear)
    })

    // Change to a clearly different year so the assertion is unambiguous.
    cy.then(() => {
      const newYear = String(Number(originalYear) + 1)
      cy.get(yearSelector).focus()
      cy.get(yearSelector)
        .type('{selectall}')
        .type(newYear, { parseSpecialCharSequences: false })

      cy.intercept('POST', '**/api/update').as('updateDatetime')
      cy.get('[data-test="modal-edit-datetime-btn-default"]').click()
      cy.wait('@updateDatetime')

      cy.then(() => { sessionStorage.clear() })
      cy.reload()
      // URL still carries ?details=true — sidebar is already open, do not toggle.
      cy.get('[data-test="detailview-sidebar"]').should('be.visible')

      // Re-open modal and verify year persisted.
      cy.get('[data-test="dateTime"]').click()
      cy.get('[data-test="modal-edit-datetime"]').should('be.visible')
      cy.get(yearSelector).should('contain', newYear)

      // Restore original year.
      cy.get(yearSelector).focus()
      cy.get(yearSelector)
        .type('{selectall}')
        .type(originalYear, { parseSpecialCharSequences: false })
      cy.intercept('POST', '**/api/update').as('restoreDatetime')
      cy.get('[data-test="modal-edit-datetime-btn-default"]').click()
      cy.wait('@restoreDatetime')

      cy.then(() => { sessionStorage.clear() })
      cy.reload()
      // URL still carries ?details=true — sidebar is already open, do not toggle.
      cy.get('[data-test="detailview-sidebar"]').should('be.visible')
      cy.get('[data-test="dateTime"]').click()
      cy.get('[data-test="modal-edit-datetime"]').should('be.visible')
      cy.get(yearSelector).should('contain', originalYear)
    })
  })
})
