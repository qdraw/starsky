'use strict';
const fs = require('fs');

exports.default = context => {
  console.log(context);

  switch (context.platform.nodeName) {
    case "darwin":
      copyFile('../starsky/starsky-osx.10.12-x64.zip', './include-starsky-darwin.zip')
      break;
    case "win32":
      copyFile('../starsky/starsky-win7-x86.zip', './include-starsky-win32.zip')
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
