import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
import { checkIfExistAndCreate } from '../helpers/create-directory-helper.cy'
import { uploadFileName1 } from './upload-filename1.cy'
const config = configFile[envFolder][envName]

describe('Upload to folder (10)', () => {
  beforeEach('Check some config settings and do them before each test (10)', () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false
    }

    // Reset storage before every new test
    cy.resetStorage()

    cy.sendAuthenticationHeader()
  })

  it('Upload to folder - Check if folder is there & create (10)', () => {
    if (!config.isEnabled) return
    checkIfExistAndCreate(config)
    cy.wait(1000)
  })

  const fileName2 = '20200822_111408.jpg'
  const fileName1 = '20200822_112430.jpg'
  const fileName3 = '20200822_134151.jpg'
  const fileName4 = '20200822_134151.mp4'

  it('Check if more menu exist (10)', {
    retries: { runMode: 1, openMode: 1 }
  }, () => {
    if (!config.isEnabled) return

    cy.visit(config.url)

    cy.get('.item.item--more').click()
    cy.get('.menu-option--input label')
  })

  it('Upload content and check if the name exist (10)', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return

    uploadFileName1(config.url, fileName1)
  })

  it('Upload more content and check if the name exist (10)', {
    retries: { runMode: 2, openMode: 2 }
  }, () => {
    if (!config.isEnabled) return

    cy.visit(config.url)

    cy.get('.item.item--more').click()
    cy.get('.menu-option--input label').click()

    const fileType = 'image/jpeg'
    const fileInput = '.menu-option--input input[type=file]'

    cy.log('next upload 2')

    cy.intercept('/starsky/api/upload', (req) => {
      // eslint-disable-next-line @typescript-eslint/no-unused-expressions
      req.headers.to = config.mkdirPath + "/" + fileName2,
      req.headers.to2 = config.mkdirPath + "/" + fileName2

    }).as('upload2')

    cy.uploadFile(fileName2, fileType, fileInput)
    cy.wait('@upload2')

    cy.log('upload 2 done')

    cy.get('[data-test=upload-files] li').should(($lis) => {
      expect($lis).to.have.length(1)
      expect($lis.eq(0)).to.contain(fileName2)
    })

    cy.intercept('/starsky/api/upload').as('upload3')
    cy.uploadFile(fileName3, fileType, fileInput)
    cy.wait('@upload3')

    cy.get('[data-test=upload-files] li').should(($lis) => {
      expect($lis).to.have.length(1)
      expect($lis.eq(0)).to.contain(fileName3)
    })

    cy.intercept('/starsky/api/upload').as('upload4')
    cy.uploadFile(fileName4, fileType, fileInput)
    cy.wait('@upload4')

    cy.get('[data-test=upload-files] li').should(($lis) => {
      expect($lis).to.have.length(1)
      expect($lis.eq(0)).to.contain(fileName4)
    })

    cy.get('.modal-exit-button').click()
  })

  it('check if list has three items (10)', () => {
    if (!config.isEnabled) return

    cy.visit(config.url)

    cy.get('.folder > div').should(($lis) => {
      expect($lis.eq(0)).to.contain(fileName2)
      expect($lis.eq(1)).to.contain(fileName1)
      expect($lis.eq(2)).to.contain(fileName3)
    })
  })
})
