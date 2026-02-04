const { scan } = require("sonarqube-scanner");
const process = require('process');
const path = require('path');
const { execSync } = require('child_process');

if (!scan) {
  console.log("sonarqube-scanner is not installed. Please install it by running `npm install -D sonarqube-scanner`");
  process.exit(1);
}

// src: https://gist.github.com/santoshshinde2012/b600d52d3bc0db2f62cf77a2044c3e05

// The URL of the SonarQube server. Defaults to http://localhost:9000
let serverUrl = process.env.STARSKY_SONAR_URL;
if (!serverUrl) {
  serverUrl = "https://sonarcloud.io";
}

const organization = process.env.STARSKY_SONAR_ORGANISATION

// The token used to connect to the SonarQube/SonarCloud server. Empty by default.
const token = process.env.STARSKY_SONAR_TOKEN;

if (!token || !organization) {
  console.error(`STARSKY_SONAR_TOKEN env is missing so skip check or:`);
  console.error(`STARSKY_SONAR_ORGANISATION env is missing so skip check`);
  process.exit(0);
}

// projectKey must be unique in a given SonarQube instance
const projectKey = "StarskyDesktop";

const isPrBuild = process.env.GITHUB_ACTIONS !== null &&
  process.env.GITHUB_JOB !== null &&
  process.env.GITHUB_BASE_REF !== null &&
  process.env.PR_NUMBER_GITHUB !== undefined &&
  process.env.PR_NUMBER_GITHUB !== "";

const githubPrNumber = process.env.PR_NUMBER_GITHUB;
const githubBaseBranch = process.env.GITHUB_BASE_REF;
const githubRepoSlug = process.env.GITHUB_REPOSITORY;

// options Map (optional) Used to pass extra parameters for the analysis.
// See the [official documentation](https://docs.sonarqube.org/latest/analysis/analysis-parameters/) for more details.
const options = {
  "sonar.projectKey": projectKey,

  // projectName - defaults to project key
  "sonar.projectName": projectKey,

  "sonar.organization": organization,

  // Path is relative to the sonar-project.properties file. Defaults to .
  "sonar.sources": "src",

  // source language
  "sonar.language": "ts",

  "sonar.typescript.tsconfigPath": "tsconfig.json",

  "sonar.exclusions": "**/build/*,**/coverage/*,**/runtime-starsky-mac-arm64/*,**/runtime-starsky-mac-x64/*,**/setup/*",

  "sonar.coverage.exclusions": "**/build/*,**/coverage/*,**/runtime-starsky-mac-arm64/*,**/runtime-starsky-mac-x64/*,**/setup/,**.spec.ts",

  "sonar.javascript.lcov.reportPaths": path.join("coverage", "lcov.info"),

  // Encoding of the source code. Default is default system encoding
  "sonar.sourceEncoding": "UTF-8",
};

let gitBranchName = "";
try {
  // Execute the git command to get the current branch name
  gitBranchName = execSync('git rev-parse --abbrev-ref HEAD').toString().trim();
  console.log("Current branch:", gitBranchName);
} catch (error) {
  console.error("Error getting branch name:", error);
}

if (!gitBranchName) {
  console.error("Set to Default Branch name: master")
  gitBranchName = "master";
}

if (!isPrBuild) {
  options["sonar.branch.name"] = gitBranchName;
}

if (isPrBuild) {
  options["sonar.pullrequest.key"] = githubPrNumber;
  options["sonar.pullrequest.branch"] = gitBranchName;
  options["sonar.pullrequest.base"] = githubBaseBranch;
  options["sonar.pullrequest.provider"] = "github";
  options["sonar.pullrequest.github.endpoint"] = "https://api.github.com/";
  options["github.repository"] = githubRepoSlug

}

// parameters for sonarqube-scanner
const params = {
  serverUrl,
  token,
  options,
};

const sonarScanner = async () => {
  console.log(serverUrl);

  if (!serverUrl) {
    console.log("SonarQube url not set. Nothing to do...");
    return;
  }

  //  Function Callback (the execution of the analysis is asynchronous).
  const callback = (result) => {
    console.log("Sonarqube scanner result:", result);
  };

  scan(params, callback);
};

sonarScanner().catch((err) => console.error("Error during sonar scan", err)).then((e) => {
  console.log(e)
});
