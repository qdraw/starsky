#!/usr/bin/node

const { join } = require("path");
const { exit } = require("process");
const { httpsPost } = require("./lib/https-post.js");
const { prefixPath } = require("./lib/prefix-path.const.js");
const { httpsGet } = require("./lib/https-get.js");

let searchPath = join(__dirname, prefixPath);

// Which company? and prefix for devops
// set: System.CollectionUri
// should contain slash at end
let systemCollectionUri = process.env.SYSTEM_COLLECTIONURI;
// systemCollectionUri = "https://dev.azure.com/wea*****/";
// System.CollectionUri = https://dev.azure.com/fabrikamfiber/
if (!systemCollectionUri) {
	console.log("SYSTEM_COLLECTIONURI is not defined");
	exit(1);
}

// Which project name?
// set: System.TeamProject
let systemTeamProject = process.env.SYSTEM_TEAM_PROJECT;
// systemTeamProject = "S***O***";
if (!systemTeamProject) {
	console.log("SYSTEM_TEAM_PROJECT is not defined");
	exit(1);
}

// Which repo
// set: Build.Repository.ID
let buildRepositoryID = process.env.BUILD_REPOSITORY_ID;
// buildRepositoryID = "My******"
// eg. 3411ebc1-d5aa-464f-9615-0b527bc66719
if (!buildRepositoryID) {
	console.log("BUILD_REPOSITORY_ID is not defined");
	exit(1);
}

// Which branch the PR is created from
// set: Build.SourceBranch
// note: refs/heads/ prefix needed
let buildSourceBranch = process.env.BUILD_SOURCE_BRANCH;
// example: "refs/heads/feature/branch"
// buildSourceBranch = "refs/heads/feature/202209_ocelot_external"

if (!buildSourceBranch) {
	console.log("BUILD_SOURCE_BRANCH is not defined");
	exit(1);
}

// Where to branch name
// set: name of master/main/dev branch -- refs/heads/ prefix needed
let targetBranch = process.env.TARGET_BRANCH;
// targetBranch = "refs/heads/development"

if (!targetBranch) {
	console.log("TARGET_BRANCH is not defined");
	exit(1);
}

// $(System.AccessToken)
let personalAccessToken =
	process.env.WEAREYOU_DEVOPS_PAT || process.env.SYSTEM_ACCESSTOKEN;
if (!personalAccessToken) {
	console.log("SYSTEM_ACCESSTOKEN or WEAREYOU_DEVOPS_PAT is not defined");
	exit(1);
}

const argv = process.argv.slice(2);

if (argv) {
	// regex: ^(\d+\.)?(\d+\.)?(\*|x|\d+)$
	for (const argItem of argv) {
		if (existsSync(argItem)) {
			searchPath = argItem;
			console.log(`use: path: ${argItem}`);
		}
	}
}

console.log(`searchPath: ${searchPath}`);

const url = `${systemCollectionUri}${systemTeamProject}/_apis/git/repositories/${buildRepositoryID}/pullrequests?searchCriteria.sourceRefName=${buildSourceBranch}&api-version=7.0`;

// First check if there is already a PR for this branch
const base64authorizationHeader =
	"Basic " +
	Buffer.from(":" + personalAccessToken).toString("base64", "utf8");
httpsGet(url, base64authorizationHeader).then((data) => {
	if (data.count === 1 && data.value.length === 1) {
		console.log("PR already exists");
		console.log(data.value[0].pullRequestId);
		console.log(data.value[0].url);
		return;
	}

	if (data.count === 0) {
		// if there is no pr, check if the branch exists
		checkIfBranchExists()
			.then(() => {
				console.log("No PR found, creating one");
				createPullRequest();
			})
			.catch((err) => {
				console.log("checking branch exists failed");
				console.log(err);
			});
		return;
	}

	console.log("Getting list of PR's FAILED");
	console.log(data);
	exit(1);
});

function checkIfBranchExists() {
	return new Promise((resolve, reject) => {
		const url = `${systemCollectionUri}${systemTeamProject}/_apis/git/repositories/${buildRepositoryID}/refs?api-version=7.0&filter=${buildSourceBranch.replace(
			"refs/",
			""
		)}`;
		console.log(`checkIfBranchExists: ${url}`);
		httpsGet(url, base64authorizationHeader).then((data) => {
			if (data.count === 1) {
				resolve();
				return;
			}
			if (data.count === 0) {
				reject(new Error("Branch does not exist"));
				return;
			}
			reject(new Error("Getting list of branches FAILED"));
		});
	});
}

function createPullRequest() {
	const content = {
		sourceRefName: buildSourceBranch,
		targetRefName: targetBranch,
		title: process.env.PR_TITLE ? process.env.PR_TITLE : "A new feature",
		description: process.env.PR_DESCRIPTION
			? process.env.PR_DESCRIPTION
			: "Updates",
		reviewers: [],
	};

	const url = `${systemCollectionUri}${systemTeamProject}/_apis/git/repositories/${buildRepositoryID}/pullrequests?api-version=6.0`;
	// exampleurl https://dev.azure.com/fabrikam/_apis/git/repositories/3411ebc1-d5aa-464f-9615-0b527bc66719/pullrequests?api-version=6.0
	// POST https://dev.azure.com/{organization}/{project}/_apis/git/repositories/{repositoryId}/pullrequests?api-version=6.0

	console.log(`going to POST to: ${url}`);
	console.log(JSON.stringify(content));
	console.log("----");

	const base64authorizationHeader =
		"Basic " +
		Buffer.from(":" + personalAccessToken).toString("base64", "utf8");
	httpsPost(url, JSON.stringify(content), base64authorizationHeader).then(
		(data) => {
			if (!data.pullRequestId) {
				console.log("PR creation FAILED");
				console.log(data);
				exit(1);
			}
			console.log("PR creation done");
			console.log(data);
			console.log("------------------");
			console.log("-> PR created: " + data.pullRequestId);
			console.log(content.url);
		}
	);

	// Error case:
	//   message: "TF401027: You need the Git 'PullRequestContribute' permission to perform this action. Details: identity 'Build\\099acf0****e47b40', scope 'repository'.",
	// https://cloudlumberjack.com/assets/img/ado-analyzer/build-service-permissions.gif
}
