
var path = require('path');
require('dotenv').config({ path: path.join(__dirname, ".env") });

var dropboxCore = require('./dropbox-core');

var dropbox = new dropboxCore(process.env.DROPBOX_ACCESSTOKEN, process.env.STARSKYIMPORTERCLI);
var dropboxFolder = '/Camera Uploads';

// Command line args
var argsArray = process.argv.slice(2);

if (argsArray.indexOf("-h") >= 0 || argsArray.indexOf("--help") >= 0) {
    console.log("Dropbox Import Help\n1. update .env file \n" +
        "2. add as arg the folder in the dropbox, default = '" + dropboxFolder + "'");

    console.log(" --path or -p > to specify the path in the dropbox");
    console.log("--colorclass > as import option int between 0-8 (no string or name)");
    console.log("--structure > where to store in the database, make sure you use the right pattern");
    console.log(" escaping structure: use 3 escape characters e.g. \\\\\\d.ext");
    process.exit(1);
}

var colorClassString = ""
for (var arg = 0; arg < argsArray.length; arg++) {
    if (argsArray[arg].toLowerCase() != "--colorclass" || (arg + 1) == argsArray.length) continue;
    colorClassString = argsArray[arg + 1];
}

// use variable dropboxFolder
for (var arg = 0; arg < argsArray.length; arg++) {
    if ((argsArray[arg].toLowerCase() == "--path" || argsArray[arg].toLowerCase() == "-p") && (arg + 1) != argsArray.length) {
        dropboxFolder = argsArray[arg + 1];
    }
}

var structure = ""
for (var arg = 0; arg < argsArray.length; arg++) {
    if (argsArray[arg].toLowerCase() != "--structure" || (arg + 1) == argsArray.length) continue;
    structure = argsArray[arg + 1];
}

console.log(">> using the -p value: " + "'" + dropboxFolder + "'");
if (colorClassString) console.log(">> using the --colorclass value: " + "'" + colorClassString + "'");
if (structure) console.log(">> using the --structure value: " + "'" + structure + "'");

dropbox.ensureExistsFile(process.env.STARSKYIMPORTERCLI).then(() => {
    dropbox.listFiles(dropboxFolder).then((entries) => {

        var entries = dropbox.filterList(entries);
        console.log('results: ', entries.length);

        dropbox.downloadList(entries).then((entries) => {
            dropbox.runStarskyList(entries, colorClassString, structure).then((entries) => {
                if (process.env.DEBUG === "true") console.log("DEBUG MODE == NO DELETE");
                if (process.env.DEBUG === "true") return;
                dropbox.removeList(entries).then((entries) => {
                    console.log("DONE :)");
                });
            });
        });
    });
});


