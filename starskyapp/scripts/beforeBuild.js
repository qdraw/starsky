'use strict';
const fs = require('fs');
const path = require('path');

exports.default = context => {
  console.log(context);

  switch (context.platform.buildConfigurationKey) {
    case "mac":
      copyFile(path.join(__dirname, '..', '..', 'starsky', 'starsky-osx.10.12-x64.zip'), path.join(__dirname, '..', 'include-starsky-mac.zip'))
      break;
    case "win":
      copyFile(path.join(__dirname, '..', '..', 'starsky', 'starsky-win7-x64.zip'), path.join(__dirname, '..' ,'include-starsky-win.zip'))
      break;
    default:
  }

  const _promises = [];
  return Promise.all(_promises);
};

function copyFile(src, dest) {
  console.log('src/dest');
  console.log(src, dest);
  console.log('src exist ' + fs.existsSync(src))
  console.log('dest exist ' + fs.existsSync(dest))

  if (!fs.existsSync(src)) {
    console.log('skipped because src is missing');
    return;
  }

  if (fs.lstatSync(src).isDirectory() ) {
    throw new Error('source is directory');
  }

  fs.copyFileSync(src, dest);
}
