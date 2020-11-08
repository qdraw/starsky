import { RequestOptions } from 'http'

function createDirectory (config) {
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
    console.log(res)
  })
}

export function checkIfExistAndCreate (config) {
  cy.request(config.checkIfDirExistApi, {
    failOnStatusCode: false,
    method: 'GET',
    headers: {
      'Content-Type': 'text/plain'
    }
  } as RequestOptions).then((data) => {
    if (data.body.searchCount === 0) {
      createDirectory(config)
    }
  })
}
