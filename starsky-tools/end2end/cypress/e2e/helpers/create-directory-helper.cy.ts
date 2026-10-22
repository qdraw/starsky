 
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
    // the search matches on a part of the path, so a folder with the same name in another
    // folder (e.g. /main/starsky-end2end-test) must not count as this folder
    const items = (data.body.fileIndexItems ?? []) as { filePath: string }[]
    if (!items.some((item) => item.filePath === config.mkdirPath)) {
      cy.log('next: create directory')
      createDirectory(config)
    }
  })
}

const defaultMkdirApi = '/starsky/api/disk/mkdir'
const testImages = ['20200822_111408.jpg', '20200822_112430.jpg', '20200822_134151.jpg']

function indexUrl (path: string): string {
  return `/api/index/?f=${path}`
}

function waitForIndex (path: string, attemptsLeft = 20): void {
  cy.request({ url: indexUrl(path), failOnStatusCode: false }).then((res) => {
    if (res.status === 200 || attemptsLeft <= 0) return
    cy.wait(500)
    waitForIndex(path, attemptsLeft - 1)
  })
}

/**
 * Specs run in an order that is not guaranteed, so a spec that needs the test folder (and
 * its sample images) can not assume that the upload spec ran before it.
 * Creates the folder and uploads the images when they are missing, and polls until indexed.
 */
export function ensureFolderWithImages (config): void {
  cy.request({
    url: config.urlMkdir,
    failOnStatusCode: false,
    method: 'GET',
    headers: { 'Content-Type': 'text/plain' }
  }).then((res) => {
    if (res.status === 200) return
    cy.log('folder is missing, create it')
    createDirectory({ ...config, mkdirApi: config.mkdirApi ?? defaultMkdirApi })
    waitForIndex(config.mkdirPath)
  })

  for (const fileName of testImages) {
    const subPath = `${config.mkdirPath}/${fileName}`
    cy.request({ url: indexUrl(subPath), failOnStatusCode: false }).then((res) => {
      if (res.status === 200) return
      cy.log(`upload missing test image ${fileName}`)
      cy.fileRequest(fileName, config.mkdirPath, 'image/jpeg')
      waitForIndex(subPath)
    })
  }
}
