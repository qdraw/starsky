
var path = require('path');
require('dotenv').config({ path: path.join(__dirname, ".env") });

var dropboxCore = require('./dropbox-core');

var dropbox = new dropboxCore(process.env.DROPBOX_ACCESSTOKEN, process.env.STARSKYIMPORTERCLI);

// Command line args
var args = process.argv.slice(2)[0];
if (args === undefined) args = "/Camera Uploads"
if (args === "--h" | args === "--help") {
    console.log("1. update .env file\n 2. add as arg the folder in the dropbox, default = 'Camera Uploads'");
    process.exit(1);
}

dropbox.ensureExistsFile(process.env.STARSKYIMPORTERCLI).then(() => {
    dropbox.listFiles(args).then((entries) => {

        var entries = dropbox.filterList(entries);
        console.log('results: ', entries.length);

        dropbox.downloadList(entries).then((entries) => {
            dropbox.runStarskyList(entries).then((entries) => {
                dropbox.removeList(entries).then((entries) => {
                    console.log("DONE :)");
                });
            });
        });
    });
});


