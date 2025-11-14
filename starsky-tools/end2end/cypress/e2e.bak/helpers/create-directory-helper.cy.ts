 
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

export function waitOnUploadIsDone(urlApiCollectionsFalse: string, index: number, max: number = 10) {
    cy.request({
      url: urlApiCollectionsFalse,
      method: "GET",
      headers: {
        "Content-Type": "text/plain",
      },
    }).then((response) => {
      expect(response.status).to.eq(200);
      cy.log(JSON.stringify(response.body.fileIndexItems));

      if (response.body.fileIndexItems.length === 4) {
        cy.log("4 items, done");
        return;
      }
      cy.wait(1500);
      index++;
      if (index < max) {
        waitOnUploadIsDone(urlApiCollectionsFalse,index, max);
      }
    });
  }