import { envName, envFolder } from "../../support/commands";
import configFile from "./config.json";
import { checkIfExistAndCreate } from "../helpers/create-directory-helper.cy";
const config = configFile[envFolder][envName];

function resetFolders() {
  cy.request({
    failOnStatusCode: false,
    method: "POST",
    url: "/starsky/api/update",
    qs: {
      f: "/starsky-end2end-test/z_test_auto_created_update;/starsky-end2end-test/z_test_auto_created",
      tags: "!delete!",
    },
  });

  cy.wait(1000);

  cy.request({
    failOnStatusCode: false,
    method: "DELETE",
    url: "/starsky/api/delete",
    qs: {
      f: "/starsky-end2end-test/z_test_auto_created_update;/starsky-end2end-test/z_test_auto_created",
    },
  });

  cy.request({
    failOnStatusCode: false,
    url: "/starsky/api/remove-cache?json=true&f=/starsky-end2end-test",
  });
}

describe("Create Rename Dir (22)", () => {
  beforeEach(
    "Check some config settings and do them before each test (22)",
    () => {
      // Check if test is enabled for current environment
      if (!config.isEnabled) {
        return false;
      }

      // Reset storage before every new test
      cy.resetStorage();

      cy.sendAuthenticationHeader();
    }
  );

  let useSystemTrashBeforeStatus = null;

  it("Create Rename Dir - Check if folder is there & create (22)", () => {
    if (!config.isEnabled) return;
    checkIfExistAndCreate(config);
    resetFolders();

    cy.sendAuthenticationHeader();

    // disable system trash
    cy.request({
      url: config.apiEnvEndpoint,
      method: "GET",
    }).then((response) => {
      useSystemTrashBeforeStatus = response.body.useSystemTrash;
      cy.log(`useSystemTrashBeforeStatus ${useSystemTrashBeforeStatus}`);
      
      cy.request({
        url: config.apiEnvEndpoint,
        method: "POST",
        form: true, // indicates the body should be form urlencoded and sets Content-Type
        body: {
          useSystemTrash: false,
        },
      });
    });
  });

  it("check env file (22)", () => {
		if (!config.isEnabled) return false;

		cy.sendAuthenticationHeader();

		cy.visit(config.apiEnvEndpoint, {
			headers: {
				"x-force-html": "true",
			},
		});
		cy.get("body")
			.should("be.visible")
			.invoke("text")
			.then((text) => {
				const parsedData = JSON.parse(text);
        cy.log(`useSystemTrash ${parsedData.useSystemTrash}`);
        expect(parsedData.useSystemTrash).to.equal(false);
			});
  });


  it("Create new folder (22)", () => {
    if (!config.isEnabled) return;

    cy.visit(config.url);

    cy.get(".item.item--more").click();
    cy.get("[data-test=mkdir]").click();

    cy.intercept("/starsky/api/disk/mkdir", (req) => {
      req.headers["content-type"] = "application/x-www-form-urlencoded";
    }).as("mkdir");
    cy.get("[data-name=directoryname]").type("z_test_auto_created");
    cy.get("[data-test=modal-archive-mkdir-btn-default]").click();
    cy.wait("@mkdir");

    cy.visit(config.url);
    cy.get(
      '[data-filepath="/starsky-end2end-test/z_test_auto_created"]'
    ).should("exist");
  });

  it("Rename new folder (22)", () => {
    if (!config.isEnabled) return;

    cy.visit(config.url + "/z_test_auto_created");

    cy.get(".item.item--more").click();
    cy.get("[data-test=rename]").click();

    cy.intercept(config.apiRename, (req) => {
      req.headers["content-type"] = "application/x-www-form-urlencoded";
    }).as("rename");

    cy.get("[data-name=foldername]").type("_update");
    cy.get(".btn.btn--default").click();

    cy.get(".modal .warning-box").should("not.exist");

    cy.wait("@rename");
    cy.request(config.urlMkdir + "/z_test_auto_created_update");

    cy.get(".folder").should("be.visible");

    cy.wait(500);
    cy.visit(config.url);

    cy.get(
      '[data-filepath="/starsky-end2end-test/z_test_auto_created_update"]'
    ).should("exist");
    cy.get(
      '[data-filepath="/starsky-end2end-test/z_test_auto_created"]'
    ).should("not.exist");
  });

  it(
    "delete it afterwards (22)",
    {
      retries: { runMode: 2, openMode: 2 },
    },
    () => {
      if (!config.isEnabled) return;

      resetFolders();

      // make sure the folder is there
      cy.request({
        method: "POST",
        url: config.apiMkdir,
        form: true,
        body: {
          f: "/starsky-end2end-test/z_test_auto_created_update",
        },
        failOnStatusCode: false,
      }).then(() => {
        cy.resetStorage();

        cy.visit(config.url);

        cy.get(".item.item--select").click();
        cy.get(
          '[data-filepath="/starsky-end2end-test/z_test_auto_created_update"] button'
        ).click();

        cy.get(".item.item--more").click();
        cy.wait(10);

        cy.intercept(config.apiMoveToTrash).as("updateToTrash");

        cy.get("[data-test=trash]").click();
        cy.wait("@updateToTrash");

        console.log("next check if is in trash");
        cy.wait(10);

        function runTest() {
          cy.visit(config.trash);
          cy.get(".item.item--select").click();
          cy.get(
            '[data-filepath="/starsky-end2end-test/z_test_auto_created_update"] button'
          ).click({ force: true });

          // menu ->
          cy.get(".item.item--more").click();
          cy.get("[data-test=delete]").click();

          // verwijder onmiddelijk
          cy.get(".modal .btn.btn--default").click();

          // item should be in the trash
          cy.get(
            '[data-filepath="/starsky-end2end-test/z_test_auto_created_update"] button'
          ).should("not.exist");

          // and not in the source folder
          cy.visit(config.url);
          cy.get(
            '[data-filepath="/starsky-end2end-test/z_test_auto_created_update"] button'
          ).should("not.exist");
        }

        cy.request(config.apiTrash).then((response) => {
          const message = response.body.fileIndexItems.find(
            (x: {filePath: string}) =>
              x.filePath === "/starsky-end2end-test/z_test_auto_created_update"
          );
          if (
            message?.filePath ===
            "/starsky-end2end-test/z_test_auto_created_update"
          ) {
            cy.log("found");
            cy.log(message);
            cy.log(message.filePath);

            runTest();
          } else {
            cy.log(" z_test_auto_created_update NOT found");
            cy.log(response.body);

            cy.wait(100);

            cy.request(config.apiTrash).then((response2) => {
              const message2 = response2.body.fileIndexItems.find(
                (x: {filePath: string}) =>
                  x.filePath === "/starsky-end2end-test/z_test_auto_created_update"
              );
              if (
                message2?.filePath ===
                "/starsky-end2end-test/z_test_auto_created_update"
              ) {
                cy.log("found");
                cy.log(message2);
                cy.log(message2.filePath);
                runTest();

              }
              else {
                expect("value").to.be("z_test_auto_created_update not found");
              }
            });

          }

        });
      });
    }
  );

  it("safe guard for other tests - if not deleted remove via the api (22)", () => {
    if (!config.isEnabled) return;

    resetFolders();

    cy.wait(1000);

    cy.intercept(config.url).as("url");

    cy.visit(config.url);

    cy.wait("@url");

    // need to wait until the page is loaded
    cy.get(".folder").should("be.visible");

    cy.get(
      '[data-filepath="/starsky-end2end-test/z_test_auto_created_update"]'
    ).should("not.exist");
    cy.get(
      '[data-filepath="/starsky-end2end-test/z_test_auto_created"]'
    ).should("not.exist");

    // z cleanup trash settings
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
