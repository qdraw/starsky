import { checkIfExistAndCreate } from "e2e/helpers/create-directory-helper.cy";
import { envName, envFolder } from "../../support/commands";
import configFile from "./config.json";
const config = configFile[envFolder][envName];

describe("Collections / stacked files (80)", () => {
  beforeEach("Check some config settings and do them before each test", () => {
    if (!config.isEnabled) {
      return false;
    }

    cy.resetStorage();
    cy.sendAuthenticationHeader();
  });

  // These two share the same fileCollectionName and therefore form a stack
  const fileNameJpg = "20200822_134151.jpg";
  const fileNameMp4 = "20200822_134151.mp4";
  const filePathJpg = `/starsky-end2end-test/${fileNameJpg}`;
  const filePathMp4 = `/starsky-end2end-test/${fileNameMp4}`;

  it("Upload jpg + mp4 with the same base name to form a stack (80)", () => {
    if (!config.isEnabled) return;

    checkIfExistAndCreate(config);

    cy.fileRequest(fileNameJpg, "/starsky-end2end-test", "image/jpeg");
    cy.fileRequest(fileNameMp4, "/starsky-end2end-test", "video/mp4");

    waitUntilBothIndexed(0);
  });

  function waitUntilBothIndexed(index: number, max: number = 10) {
    cy.request({
      url: config.urlApiCollectionsFalse,
      method: "GET",
      headers: { "Content-Type": "text/plain" },
    }).then((response) => {
      expect(response.status).to.eq(200);
      const items: Array<{ filePath: string }> = response.body.fileIndexItems ?? [];
      const hasJpg = items.some((i) => i.filePath === filePathJpg);
      const hasMp4 = items.some((i) => i.filePath === filePathMp4);
      if (hasJpg && hasMp4) return;
      cy.wait(1500);
      index++;
      if (index < max) {
        waitUntilBothIndexed(index, max);
      } else {
        expect(hasJpg && hasMp4, "both jpg and mp4 should be indexed before proceeding").to.be.true;
      }
    });
  }

  it("Archive view (collections=true): only the jpg appears, mp4 is hidden in the stack (80)", () => {
    if (!config.isEnabled) return;

    cy.visit(config.url);

    cy.get(`[data-filepath="${filePathJpg}"]`).should("exist");
    cy.get(`[data-filepath="${filePathMp4}"]`).should("not.exist");
  });

  it("Archive view (collections=false): both jpg and mp4 appear as separate items (80)", () => {
    if (!config.isEnabled) return;

    cy.visit(config.urlCollectionsFalse);

    cy.get(`[data-filepath="${filePathJpg}"]`).should("exist");
    cy.get(`[data-filepath="${filePathMp4}"]`).should("exist");
  });

  it("Detail view: opening the jpg shows both files in the collection sidebar (80)", () => {
    if (!config.isEnabled) return;

    cy.visit(`${config.url}/${fileNameJpg}`);

    // Open the labels / details panel to reveal the sidebar
    cy.get("[data-test=menu-detail-view-labels]").click();

    // Both collection members should be listed
    cy.get("[data-test=collections]").should("have.length", 2);

    // First entry is the representative file (jpg)
    cy.get("[data-test=collections]").eq(0).find("b").should("contain", fileNameJpg);

    // Second entry is the stacked sibling (mp4), marked with class box--child
    cy.get("[data-test=collections]").eq(1).should("have.class", "box--child");
    cy.get("[data-test=collections]").eq(1).find("b").should("contain", fileNameMp4);
  });

  it("Detail view: clicking the mp4 collection entry navigates to the mp4 (80)", () => {
    if (!config.isEnabled) return;

    cy.intercept("GET", "/starsky/api/info*").as("infoRequest");
    cy.visit(`${config.url}/${fileNameJpg}`);

    cy.get("[data-test=menu-detail-view-labels]").click();
    cy.wait("@infoRequest");

    cy.contains("[data-test=collections]", fileNameMp4).click();

    cy.url({ timeout: 10000 }).should("contain", fileNameMp4);
  });

  it("Last item: Clean up afterwards (80)", () => {
    if (!config.isEnabled) return;

    cy.request({
      failOnStatusCode: false,
      method: "DELETE",
      url: "/starsky/api/delete",
      qs: { f: `${filePathJpg};${filePathMp4}` },
    });
  });
});
