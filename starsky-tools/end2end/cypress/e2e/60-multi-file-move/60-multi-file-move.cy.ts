import { checkIfExistAndCreate } from "e2e/helpers/create-directory-helper.cy";
import { envName, envFolder } from "../../support/commands";
import configFile from "./config.json";
const config = configFile[envFolder][envName];

describe("Delete file from upload (50)", () => {
  beforeEach("Check some config settings and do them before each test", () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false;
    }

    // Reset storage before every new test
    cy.resetStorage();

    cy.sendAuthenticationHeader();
  });

  const fileName2 = "20200822_111408.jpg";
  const fileName1 = "20200822_112430.jpg";
  const fileName3 = "20200822_134151.jpg";

  it("clear cache & upload all files that are needed in background (60)", () => {

    
    checkIfExistAndCreate(config);

    cy.fileRequest(fileName1, "/starsky-end2end-test", "image/jpeg");
    cy.fileRequest(fileName2, "/starsky-end2end-test", "image/jpeg");
    cy.fileRequest(fileName3, "/starsky-end2end-test", "image/jpeg");

    waitOnUploadIsDone(0);

    cy.log("upload done, now create sub folder");

    const childFolderconfig = {
      checkIfDirExistApi: config.checkIfDirExistApiSubFolder,
      url: config.urlSubFolder,
      mkdirPath: config.mkdirPathSubFolder,
      searchClearCache: config.searchClearCacheSubFolder,
      urlApiCollectionsFalse: config.urlApiCollectionsFalseSubFolder,
      mkdirApi: config.mkdirApi,
    };

    cy.log(JSON.stringify(childFolderconfig));
    checkIfExistAndCreate(childFolderconfig);
    cy.log("sub folder created");
  });

  const allFilePaths60 = [
    `/starsky-end2end-test/${fileName1}`,
    `/starsky-end2end-test/${fileName2}`,
    `/starsky-end2end-test/${fileName3}`,
  ];

  function waitOnUploadIsDone(index: number, max: number = 10) {
    cy.request({
      url: config.urlApiCollectionsFalse,
      method: "GET",
      headers: { "Content-Type": "text/plain" },
    }).then((response) => {
      expect(response.status).to.eq(200);
      const items: Array<{ filePath: string }> = response.body.fileIndexItems ?? [];
      const allPresent = allFilePaths60.every((fp) => items.some((i) => i.filePath === fp));
      if (allPresent) {
        cy.log("all 3 items indexed, done");
        return;
      }
      cy.wait(1500);
      index++;
      if (index < max) {
        waitOnUploadIsDone(index, max);
      }
    });
  }

  it("Move multiple files into a subfolder and back (60)", () => {
    if (!config.isEnabled) return;
    cy.visit(config.url);
    cy.wait(500);

    cy.get(".item.item--select", { timeout: 20000 }).click();
    cy.get('[data-test="selected-0"]', { timeout: 20000 }).should("exist");

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"] button`, { timeout: 20000 })
      .should("exist")
      .click({ force: true });
    cy.get('[data-test="selected-1"]', { timeout: 20000 }).should("exist");

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName2}"] button`, { timeout: 20000 })
      .should("exist")
      .click({ force: true });
    cy.get('[data-test="selected-2"]', { timeout: 20000 }).should("exist");

    cy.get(".item.item--more").click();
    cy.get("[data-test=menu-context]").should("be.visible");
    cy.get("[data-test=move]").click();

    cy.get("[data-test=btn-child_folder]", { timeout: 15000 }).click();

    cy.get("[data-test=modal-move-file-btn-default]").should("not.be.disabled").click();

    // expect url to end with ?f=/starsky-end2end-test/child_folder
     cy.url().should('match', /\?f=\/starsky-end2end-test\/child_folder&select=20200822_112430.jpg,20200822_111408.jpg$/);

     // and undo

    cy.visit(config.urlSubFolder);
    cy.get(".item.item--select", { timeout: 20000 }).click();
    cy.get('[data-test="selected-0"]', { timeout: 20000 }).should("exist");

    cy.get(`[data-filepath="/starsky-end2end-test/child_folder/${fileName1}"] button`, {
      timeout: 20000
    })
      .should("exist")
      .click({ force: true });
    cy.get('[data-test="selected-1"]', { timeout: 20000 }).should("exist");

    cy.get(`[data-filepath="/starsky-end2end-test/child_folder/${fileName2}"] button`, {
      timeout: 20000
    })
      .should("exist")
      .click({ force: true });
    cy.get('[data-test="selected-2"]', { timeout: 20000 }).should("exist");

    cy.get(".item.item--more").click();
    cy.get("[data-test=menu-context]").should("be.visible");

    cy.get("[data-test=move]").click();

    cy.get("[data-test=parent]", { timeout: 15000 }).click();
    cy.get("[data-test=modal-move-file-btn-default]").should("not.be.disabled").click();

    // expect url to end with ?f=/starsky-end2end-test
     cy.url().should('match', /starsky-end2end-test&select=20200822_112430.jpg,20200822_111408.jpg$/);
  });

    it("Move single file into a subfolder and back (60)", () => {
    if (!config.isEnabled) return;
    cy.visit(config.url);
    cy.visit(`${config.url}/${fileName3}`);

    cy.get(".item.item--more").click();
    cy.get("[data-test=menu-context]").should("be.visible");

    cy.get("[data-test=move]").click();

    cy.get("[data-test=btn-child_folder]", { timeout: 15000 }).click();

    cy.get("[data-test=modal-move-file-btn-default]").should("not.be.disabled").click();

    // expect url to end with ?f=/starsky-end2end-test/child_folder
    cy.url({ timeout: 20000 }).should('match', /\?f=\/starsky-end2end-test\/child_folder\/20200822_134151.jpg$/);

    //  // and undo

    cy.get(".item.item--more").click();
    cy.get("[data-test=menu-context]").should("be.visible");
    cy.get("[data-test=move]").click();

    cy.get("[data-test=parent]", { timeout: 15000 }).click();
    cy.get("[data-test=modal-move-file-btn-default]").should("not.be.disabled").click();

    cy.url({ timeout: 20000 }).should('match', /\?f=\/starsky-end2end-test\/20200822_134151.jpg$/);
  });


  it("Last item: Clean up afterwards (60)", () => {
    if (!config.isEnabled) return;

    deleteFiles();
  });

  function deleteFiles() {
    // Delete via API so cleanup works regardless of where files ended up
    // (a previous test may have left files in child_folder if undo failed).
    const candidates = [
      `/starsky-end2end-test/${fileName1}`,
      `/starsky-end2end-test/${fileName2}`,
      `/starsky-end2end-test/${fileName3}`,
      `/starsky-end2end-test/child_folder/${fileName1}`,
      `/starsky-end2end-test/child_folder/${fileName2}`,
      `/starsky-end2end-test/child_folder/${fileName3}`,
      `/starsky-end2end-test/child_folder`,
    ];
    cy.request({
      failOnStatusCode: false,
      method: "DELETE",
      url: "/starsky/api/delete",
      qs: { f: candidates.join(";") },
    });
  }
});
