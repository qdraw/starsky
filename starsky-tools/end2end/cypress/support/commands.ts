
// Get current environment folder
export const envFolder = Cypress.env().configFolder

// Get current environment name
export const envName = Cypress.env().name

// checkStatusCode
declare global {
    namespace Cypress {
      interface Chainable {
        checkStatusCode: typeof checkStatusCode;
      }
    }
}

function checkStatusCode (url: string) {
  cy.request({
    method: 'GET',
    url: url,
    failOnStatusCode: false
  })
    .then((res) => {
      expect(res.status).to.eq(200)
    })
}

// Verify if page returns statuscode 200, otherwise skip test cases
Cypress.Commands.add('checkStatusCode', checkStatusCode)

// resetStorage
declare global {
    namespace Cypress {
      interface Chainable {
        resetStorage: typeof resetStorage;
      }
    }
}

function resetStorage () {
  localStorage.clear()
  sessionStorage.clear()
  cy.clearLocalStorage()
}

// Resets all storage
Cypress.Commands.add('resetStorage', resetStorage)

// Send Auth Header as cookie
declare global {
  namespace Cypress {
    interface Chainable {
      sendAuthenticationHeader: typeof sendAuthenticationHeader;
    }
  }
}

function sendAuthenticationHeader () {
  cy.request({
    method: 'POST',
    url: Cypress.config().baseUrl + '/starsky/api/account/login',
    form: false,
    followRedirect: false,
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: `Email=${Cypress.env('AUTH_USER')}&Password=${Cypress.env('AUTH_PASS')}`,
    failOnStatusCode: false
  }).then((res) => {
    expect(res.status).to.eq(200)
  })
}

Cypress.Commands.add('sendAuthenticationHeader', sendAuthenticationHeader)
