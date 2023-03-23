#!/usr/bin/node

const { httpsGet } = require("./lib/https-get.js");
const { httpsDownload } = require("./lib/https-download.js");
const fs = require("fs");
const { exit } = require("process");
const path = require("path");
const { spawnSync } = require("child_process");

// # Where is the build done from?
// set build source branch name OR build id
// set: Build.SourceBranchName
// note: refs/heads/ prefix needed
let buildSourceBranch = process.env.BUILD_SOURCE_BRANCH;
// example: "refs/heads/feature/branch"
// buildSourceBranch = "refs/heads/feature/202209_ocelot_external"

// set build source branch name OR build id
// $(Build.BuildId)
let buildId = process.env.BUILD_ID;

// Which project name?
// set: System.TeamProject
let systemTeamProject = process.env.SYSTEM_TEAM_PROJECT;
// systemTeamProject = "S***O***";
if (!systemTeamProject) {
	console.log("SYSTEM_TEAM_PROJECT is not defined");
	exit(1);
}

// # Which company? and prefix for devops
// set: System.CollectionUri
// should contain slash at end
let systemCollectionUri = process.env.SYSTEM_COLLECTIONURI;
// systemCollectionUri = "https://dev.azure.com/wea*****/";
// System.CollectionUri = https://dev.azure.com/fabrikamfiber/
if (!systemCollectionUri) {
	console.log("SYSTEM_COLLECTIONURI is not defined");
	exit(1);
}

// #Which pipeline to trigger
// https://dev.azure.com/ORG/PROJECT/_build?definitionId=380
// and then enter 380
// set: System.DefinitionId
if (process.env.ARTIFACTS_DOWNLOAD_DEFINITION_IDS) {
	azureDevopDefinitionIds =
		process.env.ARTIFACTS_DOWNLOAD_DEFINITION_IDS.split(",");
} else {
	console.log("ARTIFACTS_DOWNLOAD_DEFINITION_IDS is not defined");
	exit(1);
}

// # which artifacts to download
let status = process.env.STATUS_FILTER;
if (!status) {
	status = "completed,inProgress";
}

if (!buildSourceBranch && !buildId) {
	console.log("BUILD_SOURCE_BRANCH is not defined");
	exit(1);
}

const branchWithoutRef = buildSourceBranch?.replace("refs/heads/", "");

// set: System.AccessToken
// $(System.AccessToken)
let personalAccessToken =
	process.env.WEAREYOU_DEVOPS_PAT || process.env.SYSTEM_ACCESSTOKEN;
if (!personalAccessToken) {
	console.log("SYSTEM_ACCESSTOKEN or WEAREYOU_DEVOPS_PAT is not defined");
	exit(1);
}

// filter for artifacts to download
prefixFilter = process.env.ARTIFACTS_DOWNLOAD_PREFIX_FILTER;

var artifactsDownloadFolder = process.env.ARTIFACTS_DOWNLOAD_FOLDER;
if (!artifactsDownloadFolder) {
	artifactsDownloadFolder = __dirname;
}

if (!fs.existsSync(artifactsDownloadFolder)) {
	fs.mkdirSync(artifactsDownloadFolder);
	console.log("Created folder " + artifactsDownloadFolder);
}

var personalAccessTokenBasicAuth =
	"Basic " + Buffer.from("" + ":" + personalAccessToken).toString("base64");

for (const definitionId of azureDevopDefinitionIds) {
	let urlBuilds =
		systemCollectionUri.replace(/\/$/, "") +
		"/" +
		systemTeamProject +
		`/_apis/build/builds?api-version=7.0&$top=1&statusFilter=${status}&definitions=` +
		definitionId;
	if (branchWithoutRef) {
		urlBuilds += "&branchName=refs%2Fheads%2F" + branchWithoutRef;
	}
	if (buildId) {
		urlBuilds += "&buildId=" + buildId;
	}

	console.log("Get list of builds");
	console.log(urlBuilds);

	httpsGet(urlBuilds, personalAccessTokenBasicAuth).then((branchResult) => {
		// result can list or direct object
		if (branchResult.message || branchResult.count === 0) {
			console.log("No builds found for branch " + branchWithoutRef);
			console.log(branchResult.message);
			console.log(urlBuilds);
			return;
		}

		const buildId = branchResult?.id || branchResult.value[0].id;
		if (!buildId) {
			console.log("No build ID found");
			console.log(urlBuilds);
			return;
		}

		const urlGetArtifact =
			systemCollectionUri.replace(/\/$/, "") +
			"/" +
			systemTeamProject +
			"/_apis/build/builds/" +
			buildId +
			"/artifacts?api-version=7.0";
		httpsGet(urlGetArtifact, personalAccessTokenBasicAuth).then(
			(artifactOverviewResult) => {
				console.log(
					"Found " + artifactOverviewResult.count + " artifacts"
				);

				for (const result of artifactOverviewResult.value) {
					if (prefixFilter && !result.name.startsWith(prefixFilter)) {
						console.log(
							"Skipping artifact due filter" + result.name
						);
						continue;
					}

					const outputPath = path.join(
						artifactsDownloadFolder,
						result.name + ".zip"
					);

					httpsDownload(
						result.resource.downloadUrl,
						outputPath,
						personalAccessTokenBasicAuth
					)
						.catch((error) => {
							console.log(error);
						})
						.then(() => {
							console.log("Downloaded " + outputPath);
							try {
								const outputFolder = outputPath.replace(
									".zip",
									""
								);
								if (fs.existsSync(outputFolder)) {
									fs.unlinkSync(outputFolder);
								}

								spawnSync(
									"unzip",
									[outputPath, "-d", outputFolder],
									{
										env: process.env,
										encoding: "utf-8",
									}
								);
							} catch (error) {
								console.log(error);
							}
						});
				}
			}
		);
	});
}
