import { envFolder, envName } from "../../support/commands";
import configFile from "./config.json";
const config = configFile[envFolder][envName];

describe("env (01/03)", () => {
	it("check env file (01/03)", () => {
		if (!config.isEnabled) return false;

		cy.sendAuthenticationHeader();

		cy.visit(config.env, {
			headers: {
				"x-force-html": true,
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
				cy.log(
					`appVersionBuildDateTime: ${parsedData.appVersionBuildDateTime}`
				);
				cy.log(`databaseType: ${parsedData.databaseType}`);
				cy.log(`addMemoryCache: ${parsedData.addMemoryCache}`);
				cy.log(`isAccountRegisterOpen: ${parsedData.isAccountRegisterOpen}`);
				cy.log(
					`accountRegisterDefaultRole ${parsedData.accountRegisterDefaultRole}`
				);
				cy.log(`enablePackageTelemetry ${parsedData.enablePackageTelemetry}`);
				cy.log(
					`enablePackageTelemetryDebug ${parsedData.enablePackageTelemetryDebug}`
				);
				cy.log(
					`useDiskWatcherIntervalInMilliseconds ${parsedData.useDiskWatcherIntervalInMilliseconds}`
				);
				cy.log(`cpuUsageMaxPercentage ${parsedData.cpuUsageMaxPercentage}`);
				cy.log(
					`thumbnailGenerationIntervalInMinutes ${parsedData.thumbnailGenerationIntervalInMinutes}`
				);
				cy.log(
					`geoFilesSkipDownloadOnStartup ${parsedData.geoFilesSkipDownloadOnStartup}`
				);
				cy.log(
					`exiftoolSkipDownloadOnStartup ${parsedData.exiftoolSkipDownloadOnStartup}`
				);
				cy.log(`useSystemTrash ${parsedData.useSystemTrash}`);
			});
	});
});
