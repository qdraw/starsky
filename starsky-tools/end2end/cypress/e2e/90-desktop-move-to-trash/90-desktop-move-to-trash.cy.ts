import { checkIfExistAndCreate } from "../helpers/create-directory-helper.cy";
import { envName, envFolder } from "../../support/commands";
import configFile from "./config.json";
const config = configFile[envFolder][envName];

// Only support for system trash
describe("Desktop move to trash (100)", () => {
  let useSystemTrashBeforeStatus = null;
  let shouldRunTest = false;

  beforeEach("Check some config settings and do them before each test", () => {
    shouldRunTest = false;
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false;
    }

    // Reset storage before every new test
    cy.resetStorage();

    cy.sendAuthenticationHeader();

    cy.request({
      url: config.apiDetectToUseSystemTrash,
      method: "GET",
    }).then((response) => {
      cy.log(response.body)
      if (response.body === true) {
        // enable system trash
        cy.request({
          url: config.apiEnvEndpoint,
          method: "GET",
        }).then((response) => {
          useSystemTrashBeforeStatus = response.body.useSystemTrash;
          shouldRunTest = true;
          cy.request({
            url: config.apiEnvEndpoint,
            method: "POST",
            form: true, // indicates the body should be form urlencoded and sets Content-Type
            body: {
              useSystemTrash: true,
            },
          });
        });
      }
    });
  });

  const fileName2 = "20200822_111408.jpg";
  const fileName1 = "20200822_112430.jpg";
  const fileName3 = "20200822_134151.jpg";
  const fileName4 = "20200822_134151.mp4";

  it("clear cache & upload all files that are needed in background (90)", () => {

    if (shouldRunTest === false) {
      cy.log("shouldRunTest is false, skip test")
      return;
    }

    // clean trash
    cy.request({
      failOnStatusCode: false,
      method: "DELETE",
      url: "/starsky/api/delete",
      qs: {
        f: `/starsky-end2end-test/${fileName1};/starsky-end2end-test/${fileName2};/starsky-end2end-test/${fileName3};/starsky-end2end-test/${fileName4}`,
      },
    });
    cy.wait(500);

    checkIfExistAndCreate(config);

    cy.fileRequest(fileName4, "/starsky-end2end-test", "image/jpeg");
    cy.fileRequest(fileName1, "/starsky-end2end-test", "image/jpeg");
    cy.fileRequest(fileName2, "/starsky-end2end-test", "image/jpeg");
    cy.fileRequest(fileName3, "/starsky-end2end-test", "image/jpeg");
    waitOnUploadIsDone(0);
  });

  it("check if upload is done (90)", {
    retries: { runMode: 2, openMode: 2 }
  }, () => {

    if (shouldRunTest === false) {
      cy.log("shouldRunTest is false, skip test \n due on system trash is not supported")
      return;
    }

    waitOnUploadIsDone(0);

    cy.request(config.urlApiCollectionsFalse).then((res) => {
      expect(res.status).to.eq(200);
      // other suites can leave files in this folder, so check only the files of this suite
      expect(countOwnFiles(res.body.fileIndexItems)).to.eq(4);
    });
  });

  function countOwnFiles(items: { filePath: string }[]): number {
    const own = [fileName1, fileName2, fileName3, fileName4].map(
      (name) => `/starsky-end2end-test/${name}`
    );
    return items.filter((item) => own.includes(item.filePath)).length;
  }

  function waitOnUploadIsDone(index: number, max: number = 10) {
    cy.request({
      url: config.urlApiCollectionsFalse,
      method: "GET",
      headers: {
        "Content-Type": "text/plain",
      },
    }).then((response) => {
      expect(response.status).to.eq(200);
      cy.log(JSON.stringify(response.body.fileIndexItems));

      if (countOwnFiles(response.body.fileIndexItems) === 4) {
        cy.log("4 items, done");
        return;
      }
      cy.wait(1500);
      index++;
      if (index < max) {
        waitOnUploadIsDone(index, max);
      }
    });
  }

  it("remove item (90)", {
    retries: { runMode: 2, openMode: 2 }
  }, () => {

    if (shouldRunTest === false) {
      cy.log("shouldRunTest is false, skip test")
      return;
    }

    if (!config.isEnabled) return;
    cy.visit(config.url);

    cy.get(".item.item--select").click();
    cy.get(
      `[data-filepath="/starsky-end2end-test/${fileName1}"] button`
    ).click();

    cy.get(".item.item--more").click();
    cy.get("[data-test=trash]").click();

    // other suites can leave items (e.g. a child folder) in this folder, so check by path
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName1}"]`).should("not.exist");
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName2}"]`).should("have.length", 1);
    cy.get(`[data-filepath="/starsky-end2end-test/${fileName3}"]`).should("have.length", 1);

    cy.intercept("/starsky/api/search?json=true&t=!delete!&p=0").as(
      "trashPage"
    );

    cy.visit(config.trash);
    cy.wait("@trashPage");

    cy.get(
      `[data-filepath="/starsky-end2end-test/${fileName1}"] button`
    ).should("not.exist");

    cy.request({
      failOnStatusCode: false,
      method: "GET",
      url: config.urlApi,
    }).then((response) => {

      expect(response.status).to.eq(200);

      cy.log(JSON.stringify(response.body.fileIndexItems));
      cy.log(JSON.stringify(response.body.fileIndexItems.find(p => p.filePath === "/starsky-end2end-test/" + fileName1)));

      expect(response.body.fileIndexItems.find(p => p.filePath === "/starsky-end2end-test/" + fileName1)).to.eq(undefined);
    });
  });

  it("z cleanup trash settings (90)", () => {

    if (shouldRunTest === false) {
      cy.log("shouldRunTest is false, skip test")
      return;
    }

    cy.log("cleanup trash settings");
    cy.log(useSystemTrashBeforeStatus);

    cy.request({
      url: config.apiEnvEndpoint,
      method: "POST",
      form: true, // indicates the body should be form urlencoded and sets Content-Type
      body: {
        useSystemTrash: useSystemTrashBeforeStatus,
      },
    });
  });
});
