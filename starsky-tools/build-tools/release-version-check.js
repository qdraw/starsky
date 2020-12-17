
const packageJson = require('./package.json');

function releaseVersionCheck() {

  if (process.env.GITHUB_REF && process.env.GITHUB_REF.startsWith('refs/tags/v')) {
    const refVersion = process.env.GITHUB_REF.replace('refs/tags/v',"")
    console.log(refVersion === packageJson.version);
  }
  console.log();
}
releaseVersionCheck();
