import { config as configDotenv } from "dotenv";
import * as scanner from "sonarqube-scanner";

// src: https://gist.github.com/santoshshinde2012/b600d52d3bc0db2f62cf77a2044c3e05
// config the environment
configDotenv();

// The URL of the SonarQube server. Defaults to http://localhost:9000
const serverUrl = process.env.SONARQUBE_URL;

// The token used to connect to the SonarQube/SonarCloud server. Empty by default.
const token = process.env.SONARQUBE_TOKEN;

// projectKey must be unique in a given SonarQube instance
const projectKey = process.env.SONARQUBE_PROJECTKEY;

// options Map (optional) Used to pass extra parameters for the analysis.
// See the [official documentation](https://docs.sonarqube.org/latest/analysis/analysis-parameters/) for more details.
const options = {
  "sonar.projectKey": projectKey,

  // projectName - defaults to project key
  "sonar.projectName": "node-typescript-boilerplate",

  // Path is relative to the sonar-project.properties file. Defaults to .
  "sonar.sources": "src",

  // source language
  "sonar.language": "ts",

  "sonar.javascript.lcov.reportPaths": "coverage/lcov.info",

  // Encoding of the source code. Default is default system encoding
  "sonar.sourceEncoding": "UTF-8",
};

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

  scanner(params, callback);
};

sonarScanner().catch((err) => console.error("Error during sonar scan", err));
