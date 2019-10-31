
var path = require('path');
require('dotenv').config({ path: path.join(__dirname, ".env") });

var dropboxCore = require('./dropbox-core');

var dropbox = new dropboxCore(process.env.DROPBOX_ACCESSTOKEN, process.env.STARSKYIMPORTERCLI);

// Command line args
var argsArray = process.argv.slice(2);

if (argsArray.indexOf("-h") >= 0 || argsArray.indexOf("--help") >= 0 ) {
    console.log("Dropbox Import Help\n1. update .env file \n"+
    "2. add as arg the folder in the dropbox, default = '"+ dropboxFolder +"'");
    process.exit(1);
}

var colorClassString = ""
for (var arg = 0; arg < argsArray.length; arg++)
{
    if ( argsArray[arg].toLowerCase() != "--colorclass" || ( arg + 1 ) == argsArray.length ) continue;
    colorClassString = argsArray[arg + 1];
}

var dropboxFolder = 'Camera Uploads';
for (var arg = 0; arg < argsArray.length; arg++)
{
    if ((argsArray[arg].toLowerCase() == "--path" || argsArray[arg].toLowerCase() == "-p") && (arg + 1) != argsArray.length) {
        dropboxFolder = argsArray[arg + 1];
    }
}

console.log(">> using the -p value: " + "'" + dropboxFolder + "'");
console.log(">> using the --colorclass value: " + "'" + colorClassString + "'");

dropbox.ensureExistsFile(process.env.STARSKYIMPORTERCLI).then(() => {
    dropbox.listFiles(dropboxFolder).then((entries) => {

        var entries = dropbox.filterList(entries);
        console.log('results: ', entries.length);

        dropbox.downloadList(entries).then((entries) => {
            dropbox.runStarskyList(entries,colorClassString).then((entries) => {
                dropbox.removeList(entries).then((entries) => {
                    console.log("DONE :)");
                });
            });
        });
    });
});


