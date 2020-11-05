import { RequestOptions } from 'http'
import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
const config = configFile[envFolder][envName]

describe('Upload to folder', () => {
  beforeEach('Check some config settings and do them before each test', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()
  })

  it('Check if folder is there & create', () => {
    if (!config.isEnabled) return

    function request2 () {
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

    cy.request(config.checkIfDirExistApi, {
      failOnStatusCode: false,
      method: 'GET',
      headers: {
        'Content-Type': 'text/plain'
      }
    } as RequestOptions).then((data) => {
      if (data.body.searchCount === 0) {
        request2()
      }
    })
  })
  const fileName2 = '20200822_111408.jpg'
  const fileName1 = '20200822_112430.jpg'
  const fileName3 = '20200822_134151.jpg'

  it('Upload content and check if the name exist', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return

    cy.visit(config.url)

    cy.get('.item.item--more').click()
    cy.get('.menu-option--input label').click()

    const fileType = 'image/jpeg'
    const fileInput = '.menu-option--input input[type=file]'

    cy.uploadFile(fileName1, fileType, fileInput)
    cy.wait(1000)

    cy.get('[data-test=upload-files] li').should(($lis) => {
      expect($lis).to.have.length(1)
      expect($lis.eq(0)).to.contain(fileName1)
    })

    cy.get('.modal-exit-button').click()
  })

  it('Upload more content and check if the name exist', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return

    cy.visit(config.url)

    cy.get('.item.item--more').click()
    cy.get('.menu-option--input label').click()

    const fileType = 'image/jpeg'
    const fileInput = '.menu-option--input input[type=file]'

    cy.uploadFile(fileName2, fileType, fileInput)
    cy.wait(1000)

    cy.get('[data-test=upload-files] li').should(($lis) => {
      expect($lis).to.have.length(1)
      expect($lis.eq(0)).to.contain(fileName2)
    })

    cy.uploadFile(fileName3, fileType, fileInput)
    cy.wait(1000)

    cy.get('[data-test=upload-files] li').should(($lis) => {
      expect($lis).to.have.length(1)
      expect($lis.eq(0)).to.contain(fileName3)
    })

    cy.get('.modal-exit-button').click()
  })

  it('check if list has three items', () => {
    if (!config.isEnabled) return

    cy.visit(config.url)

    cy.get('.folder > div').should(($lis) => {
      expect($lis).to.have.length(3)
      expect($lis.eq(0)).to.contain(fileName2)
      expect($lis.eq(1)).to.contain(fileName1)
      expect($lis.eq(2)).to.contain(fileName3)
    })
  })
})
