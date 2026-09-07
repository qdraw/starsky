import { checkIfExistAndCreate } from "e2e/helpers/create-directory-helper.cy";
import { envName, envFolder } from "../../support/commands";
import configFile from "./config.json";
const config = configFile[envFolder][envName];

describe("Download files (70)", () => {
  beforeEach("Check some config settings and do them before each test", () => {
    if (!config.isEnabled) {
      return false;
    }

    cy.resetStorage();
    cy.sendAuthenticationHeader();
  });

  const fileName1 = "20200822_111408.jpg";
  const fileName2 = "20200822_112430.jpg";
  const fileName3 = "20200822_134151.jpg";
  const filePath1 = `/starsky-end2end-test/${fileName1}`;
  const filePath2 = `/starsky-end2end-test/${fileName2}`;
  const filePath3 = `/starsky-end2end-test/${fileName3}`;

  it("Upload files needed for download tests (70)", () => {
    if (!config.isEnabled) return;

    checkIfExistAndCreate(config);

    cy.fileRequest(fileName1, "/starsky-end2end-test", "image/jpeg");
    cy.fileRequest(fileName2, "/starsky-end2end-test", "image/jpeg");
    cy.fileRequest(fileName3, "/starsky-end2end-test", "image/jpeg");

    waitUntilUploaded(0);
  });

  function waitUntilUploaded(index: number, max: number = 10) {
    cy.request({
      url: config.urlApiCollectionsFalse,
      method: "GET",
      headers: { "Content-Type": "text/plain" },
    }).then((response) => {
      expect(response.status).to.eq(200);
      if (response.body.fileIndexItems.length >= 3) return;
      cy.wait(1500);
      index++;
      if (index < max) waitUntilUploaded(index, max);
    });
  }

  it("Single-file download API returns the original image (70)", () => {
    if (!config.isEnabled) return;

    cy.request({
      url: `${config.downloadPhotoApi}?f=${filePath1}&isThumbnail=false&cache=false`,
      method: "GET",
    }).then((response) => {
      expect(response.status).to.eq(200);
      expect(response.headers["content-type"]).to.match(/image\/jpeg/i);
    });
  });

  it("Download modal opens from detail view and exposes original-file link (70)", () => {
    if (!config.isEnabled) return;

    cy.visit(`${config.url}/${fileName1}`);

    cy.get(".item.item--more").click();
    cy.get("[data-test=download]").click();

    cy.get("#detailview-export-modal").should("be.visible");

    cy.get("[data-test=original]").then(($link) => {
      const href = $link.attr("href") ?? "";
      expect(href).to.include(config.downloadPhotoApi);
      expect(href).to.include("isThumbnail=false");
    });
  });

  it("Multi-file export: create-zip returns a zip hash (70)", () => {
    if (!config.isEnabled) return;

    cy.request({
      url: `${config.exportCreateZipApi}?f=${filePath1};${filePath2};${filePath3}&collections=false&thumbnail=false`,
      method: "POST",
    }).then((createResponse) => {
      expect(createResponse.status).to.eq(200);
      const zipHash = createResponse.body;
      expect(zipHash).to.be.a("string");
      expect(zipHash.length).to.be.greaterThan(0);

      pollUntilZipReady(zipHash, 0);
    });
  });

  function pollUntilZipReady(zipHash: string, index: number, max: number = 15) {
    cy.request({
      url: `${config.exportZipApi}/${zipHash}.zip?json=true`,
      method: "GET",
      failOnStatusCode: false,
    }).then((statusResponse) => {
      if (statusResponse.status === 200) {
        expect(statusResponse.body).to.eq("OK");

        cy.request({
          url: `${config.exportZipApi}/${zipHash}.zip`,
          method: "GET",
          encoding: "binary",
        }).then((zipResponse) => {
          expect(zipResponse.status).to.eq(200);
          expect(zipResponse.headers["content-type"]).to.match(/application\/zip/i);
        });
        return;
      }

      cy.wait(1500);
      index++;
      if (index < max) pollUntilZipReady(zipHash, index, max);
    });
  }

  it("Export modal opens from archive view with multiple files selected (70)", () => {
    if (!config.isEnabled) return;

    cy.visit(config.url);
    cy.wait(500);

    cy.get(".item.item--select").click();
    cy.get(`[data-filepath="${filePath1}"] button`).click({ force: true });
    cy.get(`[data-filepath="${filePath2}"] button`).click({ force: true });

    cy.get(".item.item--more").click();
    cy.get("[data-test=export]").click();

    cy.get("#detailview-export-modal").should("be.visible");
  });

  it("Last item: Clean up afterwards (70)", () => {
    if (!config.isEnabled) return;

    const paths = [filePath1, filePath2, filePath3];

    cy.request({
      failOnStatusCode: false,
      method: "DELETE",
      url: "/starsky/api/delete",
      qs: { f: paths.join(";") },
    });
  });
});
