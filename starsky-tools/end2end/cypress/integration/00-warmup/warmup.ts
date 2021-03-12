import { envName, envFolder } from '../../support/commands'
import configFile from './config.json'
const config = configFile[envFolder][envName]

describe('Warmup', () => {
  it('Warmup before starting', () => {
    if (!config.isEnabled) return false
    retry()
  })

  function retry (count = 0) {
    cy.request({
      failOnStatusCode: false,
      url: config.url
    }).then((response) => {
      if (response.status === 200 || response.status === 401) {
        return
      }
      count++
      if (count < 15) {
        retry(count)
        return
      }
      throw new Error(response.body)
    })
  }

  it('ignore loca.lt warning', () => {
    if (!config.isEnabled) return false

    cy.request({
      failOnStatusCode: false,
      url: config.url
    }).then((status) => {
      if (status.status !== 401) {
        return
      }
      cy.visit({
        failOnStatusCode: false,
        url: config.url
      })
      cy.get('.btn-primary').click()
    })
  })
})
