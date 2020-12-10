'use strict';
import * as path from 'path';
import * as fs from 'fs';

function htmlCopy() {
  const srcFolder =  path.join(__dirname, '..', '..', "src");
  const buildFolder =  path.join(__dirname, '..', '..', "build");

  console.log('-->');
  
  copyContent(srcFolder,buildFolder,/(.html|.css|.svg)$/)
}

function copyContent(source: string, target: string, match: RegExp, toRelativeFolder: string = null) {
  const files = fs.readdirSync( source );
  
  let targetFolder = target;
  if (toRelativeFolder !== null) {
    targetFolder = path.join( target, toRelativeFolder)
   
    if ( !fs.existsSync( targetFolder ) ) {
      fs.mkdirSync(targetFolder );
    }
  }

  for (const file of files) {
    var curSource = path.join( source, file );
    if ( fs.lstatSync( curSource ).isDirectory() ) {

      let newToRelative = file;
      if (toRelativeFolder != null) {
        newToRelative = path.join(toRelativeFolder, file);
      }
      copyContent(curSource, target, match, newToRelative) 
      continue; 
    }
    if (match.test(curSource)) {
      const targetFile = path.join(targetFolder,path.basename(curSource));
      console.log(targetFile);
      fs.copyFileSync(curSource, targetFile)
    }
  }
}

htmlCopy();

