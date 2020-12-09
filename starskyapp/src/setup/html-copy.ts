'use strict';
import * as path from 'path';
import * as fs from 'fs';
import { copyFolderRecursiveSync } from './copy-folder';

function htmlCopy() {
  const srcFolder =  path.join(__dirname, '..', '..', "src");
  const buildFolder =  path.join(__dirname, '..', '..', "build");

  copyFolderRecursiveSync(srcFolder, buildFolder, /.html$/)
}


console.log('--d');

htmlCopy();
console.log(process.argv.slice(2));

