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

  // The backend accepts POST /api/update before the value is written to disk by
  // the background queue, so poll /api/info until the new value is readable.
  function waitUntilApiValue (
    fieldName: string,
    expected: string,
    attempt: number = 0,
    maxAttempts: number = 20
  ) {
    cy.request({
      url: `/starsky/api/info?f=/starsky-end2end-test/${fileName}&collections=false`,
      method: 'GET',
      failOnStatusCode: false
    }).then((response) => {
      const current = String(response.body?.[0]?.[fieldName] ?? '').trim()
      if (current === expected.trim() || attempt >= maxAttempts) {
        cy.log(`${fieldName} on disk: "${current}" (attempt ${attempt})`)
        return
      }
      cy.wait(500)
      waitUntilApiValue(fieldName, expected, attempt + 1, maxAttempts)
    })
  }

  function waitUntilApiDateTimeYear (
    year: string,
    attempt: number = 0,
    maxAttempts: number = 20
  ) {
    cy.request({
      url: `/starsky/api/info?f=/starsky-end2end-test/${fileName}&collections=false`,
      method: 'GET',
      failOnStatusCode: false
    }).then((response) => {
      const current = String(response.body?.[0]?.dateTime ?? '')
      if (current.startsWith(year) || attempt >= maxAttempts) {
        cy.log(`dateTime on disk: "${current}" (attempt ${attempt})`)
        return
      }
      cy.wait(500)
      waitUntilApiDateTimeYear(year, attempt + 1, maxAttempts)
    })
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
    const fieldName = selector.replace(/^\[data-name="(.+)"\]$/, '$1')
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

    waitUntilApiValue(fieldName, newValue)

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
    cy.then(() => {
      if (originalValue.length > 0) {
        cy.get(selector)
          .type('{selectall}')
          .type(originalValue, { parseSpecialCharSequences: false })
        return
      }
      cy.get(selector).type('{selectall}{del}')
    })
    cy.intercept('POST', '**/api/update').as(`${aliasName}Restore`)
    cy.get(selector).blur()
    cy.wait(`@${aliasName}Restore`)

    cy.then(() => waitUntilApiValue(fieldName, originalValue))

    cy.then(() => { sessionStorage.clear() })
    cy.reload()
    cy.get('.item.item--labels').should('be.visible')
    cy.get('[data-test="detailview-sidebar"]').should('be.visible')
    cy.get(selector, { timeout: 20000 }).should('not.contain', newValue)
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

    // The modal renders from the detailview item; while that is still loading the
    // date is incomplete, the warning box is shown and submit stays disabled.
    // Wait for the sidebar's info API to populate the datetime before opening.
    function openDatetimeModal () {
      cy.get('[data-test="dateTime"] b', { timeout: 20000 }).invoke('text').should('match', /\d{4}/)
      cy.get('[data-test="dateTime"]').click()
      cy.get('[data-test="modal-edit-datetime"]').should('be.visible')
      cy.get('[data-test="modal-edit-datetime-non-valid"]', { timeout: 20000 }).should('not.exist')
      cy.get(yearSelector).invoke('text').should('match', /^\d{4}$/)
    }

    function setYearAndSubmit (year: string, aliasName: string) {
      cy.get(yearSelector).focus()
      cy.get(yearSelector)
        .type('{selectall}')
        .type(year, { parseSpecialCharSequences: false })
      // Blur so the onBlur→setState cycle commits the new year into React state
      // before updateDateTime() reads getDates().
      cy.get(yearSelector).blur()
      cy.get(yearSelector).should('have.text', year)

      cy.intercept('POST', '**/api/update').as(aliasName)
      cy.get('[data-test="modal-edit-datetime-btn-default"]')
        .should('not.be.disabled')
        .click()
      cy.wait(`@${aliasName}`)
      waitUntilApiDateTimeYear(year)
    }

    openDatetimeModal()
    // Capture current year before touching anything.
    cy.get(yearSelector).then((el) => {
      originalYear = el.text().trim()
      cy.log('original year: ' + originalYear)
    })

    // Change to a clearly different year so the assertion is unambiguous.
    cy.then(() => {
      const newYear = String(Number(originalYear) + 1)

      setYearAndSubmit(newYear, 'updateDatetime')

      cy.then(() => { sessionStorage.clear() })
      cy.reload()
      // URL still carries ?details=true — sidebar is already open, do not toggle.
      cy.get('[data-test="detailview-sidebar"]').should('be.visible')

      // Re-open modal and verify year persisted.
      openDatetimeModal()
      cy.get(yearSelector).should('contain', newYear)

      // Restore original year.
      setYearAndSubmit(originalYear, 'restoreDatetime')

      cy.then(() => { sessionStorage.clear() })
      cy.reload()
      // URL still carries ?details=true — sidebar is already open, do not toggle.
      cy.get('[data-test="detailview-sidebar"]').should('be.visible')
      openDatetimeModal()
      cy.get(yearSelector).should('contain', originalYear)
    })
  })
})
