import { resetStorage } from "support/commands";
import config from "../../fixtures/urls.json";

describe("env (01/03)", () => {
  it("check env file (01/03)", () => {
    resetStorage();

    cy.sendAuthenticationHeader();

    cy.visit(config.apiEnv, {
      headers: {
        "x-force-html": "true",
      },
    });

    cy.screenshot({
      capture: "fullPage",
    });

    cy.get("body")
      .should("be.visible")
      .invoke("text")
      .then((text) => {
        const parsedData = JSON.parse(text);
        cy.log(`appVersion: ${parsedData.appVersion}`);
        cy.log(`appVersionBuildDateTime: ${parsedData.appVersionBuildDateTime}`);
        cy.log(`databaseType: ${parsedData.databaseType}`);
        cy.log(`addMemoryCache: ${parsedData.addMemoryCache}`);
        cy.log(`isAccountRegisterOpen: ${parsedData.isAccountRegisterOpen}`);
        cy.log(`accountRegisterDefaultRole ${parsedData.accountRegisterDefaultRole}`);
        cy.log(`enablePackageTelemetry ${parsedData.enablePackageTelemetry}`);
        cy.log(`enablePackageTelemetryDebug ${parsedData.enablePackageTelemetryDebug}`);
        cy.log(
          `useDiskWatcherIntervalInMilliseconds ${parsedData.useDiskWatcherIntervalInMilliseconds}`
        );
        cy.log(`cpuUsageMaxPercentage ${parsedData.cpuUsageMaxPercentage}`);
        cy.log(
          `thumbnailGenerationIntervalInMinutes ${parsedData.thumbnailGenerationIntervalInMinutes}`
        );
        cy.log(`geoFilesSkipDownloadOnStartup ${parsedData.geoFilesSkipDownloadOnStartup}`);
        cy.log(`exiftoolSkipDownloadOnStartup ${parsedData.exiftoolSkipDownloadOnStartup}`);
        cy.log(`useSystemTrash ${parsedData.useSystemTrash}`);
      });
  });
});
