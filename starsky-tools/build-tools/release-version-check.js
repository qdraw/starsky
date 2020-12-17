
const packageJson = require('./package.json');
const { spawn } = require('child_process');
const path = require('path');

function releaseVersionCheck() {

  // export GITHUB_REF=refs/tags/v0.3.0
  if (process.env.GITHUB_REF && process.env.GITHUB_REF.startsWith('refs/tags/v')) {
    const refVersion = process.env.GITHUB_REF.replace('refs/tags/v',"")
    if (refVersion !== packageJson.version) {
      runChildUpdate(refVersion);
      return;
    }
  }

  if (process.env.BUILD_SOURCEBRANCH) {
    console.log(process.env.BUILD_SOURCEBRANCH);
  }

  // ADO https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml
  if (process.env.BUILD_SOURCEBRANCH && process.env.BUILD_SOURCEBRANCH.startsWith('refs/heads/release/v')) {
    console.log('=> release branches should not start with a v');
    process.exit(1);
  }

  // export BUILD_SOURCEBRANCH=refs/heads/release/0.4.2
  if (process.env.BUILD_SOURCEBRANCH && process.env.BUILD_SOURCEBRANCH.startsWith('refs/heads/release/')) {
    const refVersion = process.env.GITHUB_REF.replace('refs/heads/release/',"")
    if (refVersion !== packageJson.version) {
      runChildUpdate(refVersion);
      return;
    }
  }
}

releaseVersionCheck();

function runChildUpdate(refChildVersion) {
  const appVersionSpawn = spawn('node', [path.join(__dirname, 'app-version-update.js'), refChildVersion]);
  appVersionSpawn.stdout.on('data', (data) => {
    console.log(data);
  });

  appVersionSpawn.stderr.on('data', (data) => {
    console.error(`stderr: ${data}`);
  });

  appVersionSpawn.on('close', (code) => {
    console.log(`child process exited with code ${code}`);
    process.exit(code);
  });
}
