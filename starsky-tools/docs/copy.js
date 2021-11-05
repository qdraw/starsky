var fs = require('fs');
var path = require('path');
var outputFolderName = 'output_folder';

function copyFileSync( source, target ) {

    var targetFile = target;

    //if target is a directory a new file with the same name will be created
    if ( fs.existsSync( target ) ) {
        if ( fs.lstatSync( target ).isDirectory() ) {
            targetFile = path.join( target, path.basename( source ) );
        }
    }

    fs.writeFileSync(targetFile, fs.readFileSync(source));
}

function copyFolderRecursiveSync( source, target ) {
    var files = [];

    //check if folder needs to be created or integrated
    var targetFolder = path.join( target, path.basename( source ) );
    if ( !fs.existsSync( targetFolder ) ) {
        fs.mkdirSync( targetFolder );
    }
    else {
        fs.rmdirSync(targetFolder, { recursive: true });
        fs.mkdirSync( targetFolder );
    }

    //copy
    if ( fs.lstatSync( source ).isDirectory() ) {
        files = fs.readdirSync( source );
        files.forEach( function ( file ) {
            if (file.indexOf("node_modules") === -1 && file.indexOf("package") === -1 &&
            file.indexOf(".md") === -1 && file.indexOf("docs.js") === -1 &&
            file.indexOf("server-develop.js") === -1 &&
            file.indexOf("copy.js") === -1 &&
            file.indexOf("swagger.js") === -1
            ) {
              var curSource = path.join( source, file );
              if ( fs.lstatSync( curSource ).isDirectory() ) {
                  copyFolderRecursiveSync( curSource, targetFolder );
              } else {
                  copyFileSync( curSource, targetFolder );
              }
            }
        } );
    }


}

function copy(source, target) {
  console.log("copy " + source + " --> " + path.resolve(target, "docs"));

     // remove target folder before start
    if (fs.existsSync(target)) {
        fs.rmSync(target,{recursive: true});
    }
    fs.mkdirSync(target);


  copyFolderRecursiveSync( source, target );

  fs.writeFileSync(path.join(target, "docs", "readme.md"), '# Auto generated folder \nOpen `index.html` to see the documentation. ' +
  'Do not edit this folder,\nDocs is generated by `starsky-tools/docs` ');
  fs.writeFileSync(path.join(target, "docs", "__do_NOT_edit_this_folder__"), 'Open `index.html` to see the documentation. Do not edit this folder,' +
  '\nDocs is generated by starsky-tools/docs');
}


var myArgs = process.argv.slice(2);

if (myArgs && myArgs.length === 1 && fs.lstatSync(  myArgs[0] ).isDirectory() ) {
  // use the parent folder as arg
  copy(path.join(__dirname, outputFolderName), myArgs[0]);
}
else {
  copy(path.join(__dirname, outputFolderName), path.join("../","../","docs"));
  console.log("has run default option: ignore args: enter the full parent folder as arg");
}
