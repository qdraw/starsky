 
import { RequestOptions } from 'http'

function createDirectory (config): void {
  cy.request({
    method: 'POST',
    url: config.mkdirApi,
    form: false,
    followRedirect: false,
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: `f=${config.mkdirPath}`,
    failOnStatusCode: false
  }).then((res) => {
    cy.log('mkdir done')
    cy.log(res.body)
    cy.log(res.status.toString())
    cy.wait(500);
  })
}

export function checkIfExistAndCreate (config): void {
  cy.request('POST', config.searchClearCache)
  cy.request(config.checkIfDirExistApi, {
    failOnStatusCode: false,
    method: 'GET',
    headers: {
      'Content-Type': 'text/plain'
    }
  } as RequestOptions).then((data) => {
    // cy.log(JSON.stringify(data))
    if (data.body.searchCount === 0) {
      cy.log('next: create directory')
      createDirectory(config)
    }
  })
}
