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
  // The fixture file has no EXIF datetime, so fields start empty and the
  // warning box shows on first open. We fill all six fields with a known
  // date, submit, then cycle the year to verify persistence and restore.
  // retries=3 because the modal submit → reload → verify cycle is the most
  // susceptible step to slow async writes on CI.
  it('edit datetime via modal, verify persistence, restore (45)', {
    retries: { runMode: 3, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return
    openWithSidebar()

    const modalSelector = '[data-test="modal-edit-datetime"]'
    const yearSelector = `${modalSelector} [data-name="year"]`
    const monthSelector = `${modalSelector} [data-name="month"]`
    const dateSelector = `${modalSelector} [data-name="date"]`
    const hourSelector = `${modalSelector} [data-name="hour"]`
    const minuteSelector = `${modalSelector} [data-name="minute"]`
    const secSelector = `${modalSelector} [data-name="sec"]`

    // Known full datetime for a file that has no EXIF data.
    // Using the date encoded in the fixture filename as a mnemonic.
    const baseYear = '2019'
    const changedYear = '2020'

    function openModal () {
      cy.get('[data-test="dateTime"]').click()
      cy.get(modalSelector).should('be.visible')
    }

    function typeInField (selector: string, value: string) {
      cy.get(selector).focus()
      cy.get(selector).type('{selectall}').type(value, { parseSpecialCharSequences: false })
      cy.get(selector).blur()
      // Wait for React to commit the state update from onBlur before typing
      // into the next field. On Windows CI the re-render can race with the
      // next field's focus and wipe the just-typed value.
      cy.get(selector).should('have.text', value)
    }

    // Fill every field so the warning disappears and the submit button enables.
    // Called only for the first open when the file has no existing datetime.
    function fillAllFields (year: string) {
      typeInField(yearSelector, year)
      typeInField(monthSelector, '08')
      typeInField(dateSelector, '22')
      typeInField(hourSelector, '13')
      typeInField(minuteSelector, '41')
      typeInField(secSelector, '51')
      cy.get('[data-test="modal-edit-datetime-non-valid"]', { timeout: 10000 }).should('not.exist')
      cy.get('[data-test="modal-edit-datetime-btn-default"]').should('not.be.disabled')
    }

    function changeYearAndSubmit (year: string, aliasName: string) {
      typeInField(yearSelector, year)
      cy.get(yearSelector).should('have.text', year)
      cy.get('[data-test="modal-edit-datetime-non-valid"]').should('not.exist')
      cy.intercept('POST', '**/api/update').as(aliasName)
      cy.get('[data-test="modal-edit-datetime-btn-default"]').should('not.be.disabled').click()
      cy.wait(`@${aliasName}`)
      waitUntilApiDateTimeYear(year)
    }

    // ── Step 1: set a complete datetime from scratch (file had no EXIF) ──────
    openModal()
    fillAllFields(baseYear)
    cy.intercept('POST', '**/api/update').as('updateDatetime1')
    cy.get('[data-test="modal-edit-datetime-btn-default"]').click()
    cy.wait('@updateDatetime1')
    waitUntilApiDateTimeYear(baseYear)

    cy.then(() => { sessionStorage.clear() })
    cy.reload()
    cy.get('[data-test="detailview-sidebar"]').should('be.visible')

    // ── Step 2: change year to changedYear, verify persistence ───────────────
    openModal()
    cy.get(yearSelector).should('have.text', baseYear)
    changeYearAndSubmit(changedYear, 'updateDatetime2')

    cy.then(() => { sessionStorage.clear() })
    cy.reload()
    cy.get('[data-test="detailview-sidebar"]').should('be.visible')

    openModal()
    cy.get(yearSelector).should('have.text', changedYear)

    // ── Step 3: restore to baseYear ──────────────────────────────────────────
    changeYearAndSubmit(baseYear, 'restoreDatetime')

    cy.then(() => { sessionStorage.clear() })
    cy.reload()
    cy.get('[data-test="detailview-sidebar"]').should('be.visible')

    openModal()
    cy.get(yearSelector).should('have.text', baseYear)
  })
})
