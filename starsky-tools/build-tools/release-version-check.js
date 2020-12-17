
const packageJson = require('./package.json');
const { spawn } = require('child_process');
const path = require('path');

function releaseVersionCheck() {

// export GITHUB_REF=refs/tags/v0.3.0
  if (process.env.GITHUB_REF && process.env.GITHUB_REF.startsWith('refs/tags/v')) {
    const refVersion = process.env.GITHUB_REF.replace('refs/tags/v',"")
    if (refVersion !== packageJson.version) {
      const r = spawn('node', [path.join(__dirname, 'app-version-update.js'), refVersion]);
      r.stdout.on('data', (data) => {
        console.log(`stdout: ${data}`);
      });

      r.stderr.on('data', (data) => {
        console.error(`stderr: ${data}`);
      });
      ls.on('close', (code) => {
        console.log(`child process exited with code ${code}`);
      });

    }
    console.log();
  }
  console.log();
}
releaseVersionCheck();
