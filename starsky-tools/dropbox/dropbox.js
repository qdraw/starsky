
var path = require('path');
require('dotenv').config({ path: path.join(__dirname, ".env") });

var dropboxCore = require('./dropbox-core');

var dropbox = new dropboxCore(process.env.DROPBOX_ACCESSTOKEN, process.env.STARSKYIMPORTERCLI);

dropbox.ensureExistsFile(process.env.STARSKYIMPORTERCLI).then(() => {
    dropbox.listFiles("/test").then((entries) => {

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


