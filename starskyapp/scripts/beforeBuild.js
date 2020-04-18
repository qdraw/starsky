'use strict';
const fs = require('fs');

exports.default = context => {
  console.log(context);

  switch (context.platform.buildConfigurationKey) {
    case "mac":
      copyFile('../starsky/starsky-osx.10.12-x64.zip', './include-starsky-mac.zip')
      break;
    case "win":
      copyFile('../starsky/starsky-win7-x86.zip', './include-starsky-win.zip')
      break;
    default:
  }

  const _promises = [];
  return Promise.all(_promises);
};

function copyFile(src, dest) {
  console.log(src, dest);
  fs.copyFileSync(src, dest);
}
