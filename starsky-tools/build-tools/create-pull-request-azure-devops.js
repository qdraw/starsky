#!/usr/bin/node

const { spawnSync } = require('child_process');
const { join } = require("path");
const { exit } = require('process');
const { httpsPost } = require('./lib/https-post.js');
const { prefixPath } = require("./lib/prefix-path.const.js");

let searchPath = join(__dirname, prefixPath);

// Which company? and prefix for devops
// set: System.CollectionUri 
let systemCollectionUri = process.env.SYSTEM_COLLECTIONURI;
// systemCollectionUri = "https://dev.azure.com/wea*****/";
// System.CollectionUri = https://dev.azure.com/fabrikamfiber/
if (!systemCollectionUri) {
    console.log('SYSTEM_COLLECTIONURI is not defined');
    exit(1);
}

// Which project name?
// set: System.TeamProject
let systemTeamProject = process.env.SYSTEM_TEAM_PROJECT;
// systemTeamProject = "S***O***";
if (!systemTeamProject) {
    console.log('SYSTEM_TEAM_PROJECT is not defined');
    exit(1);
}


// Which repo
// set: Build.Repository.ID
let buildRepositoryID = process.env.BUILD_REPOSITORY_ID; 
// buildRepositoryID = "My******"
// eg. 3411ebc1-d5aa-464f-9615-0b527bc66719
if (!buildRepositoryID) {
    console.log('BUILD_REPOSITORY_ID is not defined');
    exit(1);
}


// Which branch it is from
// set: Build.SourceBranch
let buildSourceBranch = process.env.BUILD_SOURCE_BRANCH;
// example: "refs/heads/feature/branch"
// buildSourceBranch = "refs/heads/feature/202209_ocelot_external"

if (!buildSourceBranch) {
    console.log('BUILD_SOURCE_BRANCH is not defined');
    exit(1);
}


// Where to branch name
// set: name of master/main/dev branch -- refs/heads/ prefix needed
let targetBranch = process.env.TARGET_BRANCH;
// targetBranch = "refs/heads/development"

if (!targetBranch) {
    console.log('TARGET_BRANCH is not defined');
    exit(1);
}

let personalAccessToken = process.env.WEAREYOU_DEVOPS_PAT;
if (!personalAccessToken) {
    console.log('PAT is not defined');
    exit(1);
}

const argv = process.argv.slice(2);

if (argv) {
	// regex: ^(\d+\.)?(\d+\.)?(\*|x|\d+)$
	for (const argItem of argv) {
		if (existsSync(argItem)) {
			searchPath = argItem;
			console.log(`use: path: ${argItem}`)
		}
	}
}

console.log(searchPath);

const gitVersion = spawnSync('git', ['--version'], {
    cwd: searchPath,
    env: process.env,
    encoding: 'utf-8'
});

if (gitVersion.stdout.indexOf("git version") === -1) {
    console.error("git not found");
    exit(1);
}

const gitStatusPorcelain = spawnSync('git', ['status', '--porcelain'], {
    cwd: searchPath,
    env: process.env,
    encoding: 'utf-8'
});

if (!gitStatusPorcelain.stdout) {
    console.log("no changes");
    exit(0);
}

console.log("files that are changed");
console.log(gitStatusPorcelain.stdout);

const content = {
  "sourceRefName": buildSourceBranch,
  "targetRefName": targetBranch,
  "title": "A new feature",
  "description": "Updates",
  "reviewers": []
};

const url = `${systemCollectionUri}${systemTeamProject}/_apis/git/repositories/${buildRepositoryID}/pullrequests?api-version=6.0`;
// exampleurl https://dev.azure.com/fabrikam/_apis/git/repositories/3411ebc1-d5aa-464f-9615-0b527bc66719/pullrequests?api-version=6.0
// POST https://dev.azure.com/{organization}/{project}/_apis/git/repositories/{repositoryId}/pullrequests?api-version=6.0


console.log($`going to POST to: ${url}`);
console.log(JSON.stringify(content));
console.log("----");

const base64authorizationHeader = 'Basic ' + Buffer.from(":" + personalAccessToken).toString('base64', 'utf8');
httpsPost(url, JSON.stringify(content),base64authorizationHeader).then((data)=>{
    if (!data.pullRequestId) {
        console.log("PR creation FAILED");
        console.log(data);
        exit(1);
    }
    console.log("PR creation done");
    console.log(data);
});

