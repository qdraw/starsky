import { checkIfExistAndCreate } from "../helpers/create-directory-helper.cy";
import { envName, envFolder } from "../../support/commands";
import configFile from "./config.json";
const config = configFile[envFolder][envName];

describe("Delete file from upload (50)", () => {

  let useSystemTrashBeforeStatus = null;
  beforeEach("Check some config settings and do them before each test", () => {
    // Check if test is enabled for current environment
    if (!config.isEnabled) {
      return false;
    }

    // Reset storage before every new test
    cy.resetStorage();

    cy.sendAuthenticationHeader();

    // disable system trash
    cy.request({
      url: config.apiEnvEndpoint,
      method: "GET",
    }).then((response) => {
      useSystemTrashBeforeStatus = response.body.useSystemTrash
      cy.request({
        url: config.apiEnvEndpoint,
        method: "POST",
        form: true, // indicates the body should be form urlencoded and sets Content-Type
        body: {
          useSystemTrash: false,
        }
      })
    });

  });

  const fileName2 = "20200822_111408.jpg";
  const fileName1 = "20200822_112430.jpg";
  const fileName3 = "20200822_134151.jpg";
  const fileName4 = "20200822_134151.mp4";

  it("clear cache & upload all files that are needed in background (50)", () => {
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

  it("check if upload is done (50)", () => {
    cy.request(config.urlApiCollectionsFalse).then((res) => {
      expect(res.status).to.eq(200);
      expect(res.body.fileIndexItems.length).to.eq(4);
    });
  });

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

      if (response.body.fileIndexItems.length === 4) {
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

  it("remove collection item, but not the other file (50)", () => {
    cy.visit(config.urlVideoItemCollectionsFalse);

    cy.get(".item.item--more").click();
    cy.get("[data-test=trash]").click();

    cy.visit(config.url);
    cy.get(".folder > div").should(($lis) => {
      expect($lis).to.have.length(3);
    });

    waitFileInTrash(0, `/starsky-end2end-test/${fileName4}`);

    cy.log(`go to: ${config.trash}`);

    cy.intercept("/starsky/api/search?json=true&t=!delete!&p=0").as(
      "trashPage"
    );
    cy.visit(config.trash);
    cy.wait("@trashPage");

    cy.get(".item.item--select").click();
    cy.get(
      `[data-filepath="/starsky-end2end-test/${fileName4}"] button`
    ).click();

    // more menu and delete
    cy.get(".item.item--more").click();
    cy.get("[data-test=delete]").click();

    // verwijder onmiddelijk
    cy.intercept("/starsky/api/delete").as("delete4");
    cy.get(".modal .btn.btn--default").click();
    cy.wait("@delete4");

    cy.wait(500);

    // test 50
    cy.request(config.urlApiCollectionsFalse).then((res) => {
      expect(res.status).to.eq(200);
      expect(res.body.fileIndexItems.length).to.eq(3);
    });

    cy.visit(config.url);

    cy.get(`[data-filepath="/starsky-end2end-test/${fileName3}"]`);

    cy.get(".folder > div").should(($lis) => {
      expect($lis).to.have.length(3);
    });
  });

  function waitFileInTrash(index: number, filePath: string, max: number = 15) {
    cy.request({
      url: "/api/search/trash",
      method: "GET",
      headers: {
        "Content-Type": "text/plain",
      },
    }).then((response) => {
      expect(response.status).to.eq(200);
      if (response.body.fileIndexItems.length) {
        cy.log(JSON.stringify(response.body.fileIndexItems));
      }

      for (const item of response.body.fileIndexItems) {
        if (item.filePath === filePath) {
          cy.log("in trash end");
          return;
        }
      }
      cy.wait(1500);
      index++;
      if (index < max) {
        waitFileInTrash(index, filePath, max);
      }
    });
  }

  it("remove first on to trash and undo afterwards (50)", () => {
    if (!config.isEnabled) return;
    cy.visit(config.url);

    cy.get(".item.item--select").click();
    cy.get(
      `[data-filepath="/starsky-end2end-test/${fileName1}"] button`
    ).click();

    cy.get(".item.item--more").click();
    cy.get("[data-test=trash]").click();

    cy.get(".folder > div").should(($lis) => {
      expect($lis).to.have.length(2);
    });

    waitFileInTrash(0, `/starsky-end2end-test/${fileName1}`);
    cy.visit(config.trash);

    cy.log("view trash");

    cy.get(".item.item--select").click();

    cy.get(
      `[data-filepath="/starsky-end2end-test/${fileName1}"] button`
    ).click();

    cy.log("next: click more and restore from trash");

    // restore
    cy.get(".item.item--more").click();

    cy.intercept("/starsky/api/replace").as("replace");
    cy.get("[data-test=restore-from-trash]").click();
    cy.wait("@replace");

    cy.log("next: should be restored");

    // item should be in the trash
    cy.get(
      `[data-filepath="/starsky-end2end-test/${fileName1}"] button`
    ).should("not.exist");

    // display keys
    cy.window().then((win) => {
      const keys = Object.keys(win.sessionStorage);
      keys.forEach((key) => {
        cy.log(key);
      });
    });

    // remove keys when exists, switching can be so fast that the removal isn't done
    cy.window().then((win) => {
      const keys = Object.keys(win.sessionStorage);
      keys.forEach((key) => {
        if (key.startsWith("starsky")) {
          win.sessionStorage.removeItem(key);
          cy.log("key removed");
        }
      });
    });

    cy.wait(500);
    cy.intercept("/starsky/api/index?f=/starsky-end2end-test").as("e2e_page");
    cy.visit(config.url);
    cy.wait("@e2e_page");

    if (envName === "local") {
      cy.request("/starsky/api/index?f=/starsky-end2end-test").then((res) => {
        cy.log(
          JSON.stringify(
            res.body.fileIndexItems.find((p) => p.fileName === fileName1)
          )
        );
      });
    }

    cy.request("/starsky/api/memory-cache-debug").then((res) => {
      cy.log(JSON.stringify(res.body));
    });

    cy.request("/starsky/api/index?f=/starsky-end2end-test").then((res) => {
      expect(res.status).to.eq(200);
      expect(
        JSON.stringify(
          res.body.fileIndexItems.find((p) => p.fileName === fileName1)
        )
      ).contain(fileName1);
    });

    cy.get(".folder > div").contains(fileName1);
  });

  it("remove item and remove from trash (50)", () => {
    if (!config.isEnabled) return;
    cy.visit(config.url);

    cy.get(".item.item--select").click();
    cy.get(
      `[data-filepath="/starsky-end2end-test/${fileName1}"] button`
    ).click();

    cy.get(".item.item--more").click();
    cy.get("[data-test=trash]").click();

    cy.get(".folder > div").should(($lis) => {
      expect($lis).to.have.length(2);
    });

    waitFileInTrash(0, `/starsky-end2end-test/${fileName1}`);
    cy.visit(config.trash);

    cy.get(".item.item--select").click();
    cy.get(
      `[data-filepath="/starsky-end2end-test/${fileName1}"] button`
    ).click();

    cy.get(".item.item--more").click();
    cy.get("[data-test=delete]").click();

    // verwijder onmiddelijk
    cy.intercept("/starsky/api/delete").as("delete1");
    cy.get(".modal .btn.btn--default").click();
    cy.wait("@delete1");

    // item should be in the trash
    cy.get(
      `[data-filepath="/starsky-end2end-test/${fileName1}"] button`
    ).should("not.exist");

    cy.visit(config.url);

    cy.get(".folder > div").should(($lis) => {
      expect($lis).to.have.length(2);
    });
  });

  it("z cleanup trash settings (50)", () => {
    cy.log("cleanup trash settings");
    cy.log(useSystemTrashBeforeStatus);
    
    cy.request({
      url: config.apiEnvEndpoint,
      method: "POST",
      form: true, // indicates the body should be form urlencoded and sets Content-Type
      body: {
        useSystemTrash: useSystemTrashBeforeStatus,
      }
    })

  });
});
