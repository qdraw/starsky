export function uploadFileName1 (url: string, fileName1: string, check = true) {
  cy.visit(url)

  cy.get('.item.item--more').click()
  cy.get('.menu-option--input label')

  const fileType = 'image/jpeg'
  const fileInput = '.menu-option--input input[type=file]'

  cy.uploadFile(fileName1, fileType, fileInput)

  cy.wait(1000)

  cy.get('[data-test=upload-files] li').should(($lis) => {
    expect($lis).to.have.length(1)
    expect($lis.eq(0)).to.contain(fileName1)
  })

  cy.get('.modal-exit-button').click()
}
