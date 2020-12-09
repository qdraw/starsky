'use strict';
import * as path from 'path';
import * as fs from 'fs';

exports.default = (context: { platform: { buildConfigurationKey: string; }; }) => {
  console.log(context);

  switch (context.platform.buildConfigurationKey) {
    case "mac":
      copyWithId('osx.10.12-x64', 'runtime-starsky-mac')
      break;
    case "win":
        copyWithId('win7-x64', 'runtime-starsky-win')
      break;
    default:
  }

  const _promises: readonly any[] = [];
  return Promise.all(_promises);
};

function copyWithId(identifier: string, toName: string) {
    const from = path.join(__dirname, '..', '..', '..', 'starsky', identifier );
    const to =  path.join(__dirname, '..', '..');
    
    copyFolderRecursiveSync(from,to);

    const afterCopyPath =  path.join(__dirname, '..', '..', identifier);
    const afterCopyTo =  path.join(__dirname, '..', '..', toName);

    try {
        fs.rmdirSync(afterCopyTo, { recursive: true });
    } catch (err) {
        console.log(err);
    }

    fs.renameSync(afterCopyPath, afterCopyTo)
}

function copyFolderRecursiveSync( source: string, target : string ) {
    var files = [];

    // Check if folder needs to be created or integrated
    var targetFolder = path.join( target, path.basename( source ) );
    if ( !fs.existsSync( targetFolder ) ) {
        fs.mkdirSync( targetFolder );
    }

    // Copy
    if ( fs.lstatSync( source ).isDirectory() ) {
        files = fs.readdirSync( source );
        files.forEach( function ( file ) {
            var curSource = path.join( source, file );
            if ( fs.lstatSync( curSource ).isDirectory() ) {
                copyFolderRecursiveSync( curSource, targetFolder );
            } else {
                copyFileSync( curSource, targetFolder );
            }
        } );
    }
}

function copyFileSync( source: string, target : string ) {

    var targetFile = target;

    // If target is a directory, a new file with the same name will be created
    if ( fs.existsSync( target ) ) {
        if ( fs.lstatSync( target ).isDirectory() ) {
            targetFile = path.join( target, path.basename( source ) );
        }
    }

    fs.writeFileSync(targetFile, fs.readFileSync(source));
}

