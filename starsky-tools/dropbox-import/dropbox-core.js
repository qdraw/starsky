var path = require('path');
var fs = require('fs');
const axios = require('axios');

const util = require('util');
const exec = util.promisify(require('child_process').exec);

module.exports = class Dropbox {

    constructor(access_token, starskyCli) {
        this.access_token = access_token;
        this.starskyCli = starskyCli;

        if (!access_token) {
            throw Error("missing DROPBOX_ACCESSTOKEN");
        }

        if (!starskyCli) {
            throw Error("add STARSKYIMPORTERCLI");
        }

        // Make sure the output directories exist
        this.getRights();
    }

    /** 
     * Check if Folder exist
     *  */
    ensureExistsFolder(path, mask, cb) {
        if (typeof mask == 'function') { // allow the `mask` parameter to be optional
            cb = mask;
            mask = parseInt('0777', 8);
        }
        fs.mkdir(path, mask, function (err) {
            if (err) {
                if (err.code == 'EEXIST') cb(null); // ignore the error if the folder already exists
                else cb(err); // something else went wrong
            } else cb(null); // successfully created folder
        });
    }

    /**
     * To store the downloaded files
     */
    getTempFolder() {
        return path.join(__dirname, "temp");
    }

    /**
     * Check if the temp folder has read rights
     */
    getRights() {
        this.ensureExistsFolder(this.getTempFolder(), parseInt('0744', 8), function (err) {
            if (err) console.log(err);// handle folder creation error
        });
        this.ensureExistsFile(this.starskyCli);
    }

    ensureExistsFile(libfolder) {
        return new Promise((resolve, reject) => {
            fs.stat(libfolder, function (err, stats) {
                if (err) {
                    if (err.code === 'ENOENT') {
                        console.log("PLEASE UPDATE STARSKY IN CONFIG");
                        console.log(libfolder);
                        process.exit(1);
                    }
                }
                resolve();
            })
        });
    }

    /**
     * Default settings to send to Dropbox
     */
    requestOptions() {
        return {
            url: this.base_url,
            method: "GET",
            headers: {
                'User-Agent': 'MS FrontPage Express',
                'Authorization': 'Bearer ' + this.access_token,
                'Content-Type': 'application/json'
            },
        }
    };

    /**
     * Get a list of all files in the selected folder
     * @param {string} dropboxfolder the path to the remote dropbox folder
     */
    listFiles(dropboxfolder) {
        this.getRights();
        return new Promise((resolve, reject) => {
            (async () => {
                var response = await this.listQuery(dropboxfolder);
                if (!response.data.has_more) {
                    resolve(response.data.entries);
                }
                while (response.data.has_more) {
                    response = await this.listQueryCursor(response);
                }
                resolve(response.data.entries);
            })()
        });
    }

    /**
     * Do a new response because the previous has_more on true
     * @param {[]} inputResponse the response that is done before
     */
    listQueryCursor(inputResponse) {
        var cursorQuery = '{"cursor":"' + inputResponse.data.cursor + '"}';
        var listQueryRequestOptions = this.requestOptions();
        listQueryRequestOptions.url = 'https://api.dropboxapi.com/2/files/list_folder/continue';
        listQueryRequestOptions.method = "POST";
        listQueryRequestOptions.data = JSON.parse(cursorQuery);

        return new Promise((resolve, reject) => {
            axios(listQueryRequestOptions).then((response) => {
                response.data.entries = response.data.entries.concat(inputResponse.data.entries)
                resolve(response)
            }).catch(function (thrown) {
                console.log(thrown);
                resolve(false);
            });
        })
    }

    /**
     * Do the first request to the folder to get the first results (can ben complete or incomplete)
     * @param {string} dropboxfolder the path of the folder
     */
    listQuery(dropboxfolder) {
        var formquery = '{"path":"' + dropboxfolder + '"}';
        var listQueryRequestOptions = this.requestOptions();
        listQueryRequestOptions.url = "https://api.dropboxapi.com/2/files/list_folder";
        listQueryRequestOptions.method = "POST";
        listQueryRequestOptions.data = JSON.parse(formquery);

        return new Promise((resolve, reject) => {
            axios(listQueryRequestOptions).then((response) => {
                resolve(response)
            }).catch(function (thrown) {
                console.log(thrown);

                resolve(false);
            });
        })
    }


    /**
     * Remove a list of items from disk and server
     * @param {array} entries list of items
     */
    removeList(entries) {
        return new Promise((resolve, reject) => {
            (async () => {
                var index = 0;
                while (index != entries.length) {
                    await this.removeSingleFile(entries[index]);
                    index++;
                }
                resolve(entries);
            })()
        })
    }

    /**
     * return only items that match a few extensions
     * @param {array} entries All items
     */
    filterList(entries) {
        var resultEntries = [];
        var whitelist = "jpg,tiff,dng,arw".split(",")
        entries.forEach(element => {

            for (let index = 0; index < whitelist.length; index++) {
                const extenstion = whitelist[index];
                if (element.name.endsWith(extenstion)) {
                    resultEntries.push(element)
                };
            }
        });
        return resultEntries;
    }

    /**
     * Download this entire list
     * @param {array} entries list of items
     */
    downloadList(entries) {
        return new Promise((resolve, reject) => {
            (async () => {
                var index = 0;
                while (index != entries.length) {
                    await this.downloadBinarySingleFile(entries[index]);
                    index++;
                }
                resolve(entries);
            })()
        })
    }

    /**
     * Run the StarskyImporterCli with a single file
     * @param {entries} entries list of items
     * @param {colorClassString} to overwrite the colorclass
     * @param {structure} to set the structure
     */
    runStarskyList(entries, colorClassString, structure) {
        return new Promise((resolve, reject) => {
            (async () => {
                var index = 0;
                while (index != entries.length) {
                    var filePath = path.join(this.getTempFolder(), entries[index].name);

                    var exe = this.starskyCli + ' -p \"' + filePath + "\"" + " --colorclass " + colorClassString;
                    if (structure) exe += " --structure " + structure;

                    const { stdout, stderr } = await exec(exe);
                    if (stderr) {
                        console.log(stderr);
                        reject();
                    }
                    console.log(stdout);
                    index++;
                }
                resolve(entries);
            })()
        })
    }

    /**
     * Download this single element to a temp folder
     * @param {object} element single element
     */
    downloadBinarySingleFile(element) {

        this.getRights();

        var downloadFileRequestOptions = this.requestOptions(0);
        downloadFileRequestOptions.url = 'https://content.dropboxapi.com/2/files/download';
        downloadFileRequestOptions.responseType = 'stream'
        downloadFileRequestOptions.method = "POST";
        downloadFileRequestOptions.headers['Content-Type'] = 'application/octet-stream';
        downloadFileRequestOptions.headers['Dropbox-API-Arg'] = '{"path":"' + element.path_display + '"}';

        var filePath = path.join(this.getTempFolder(), element.name);
        console.log(filePath);

        return new Promise((resolve, reject) => {
            axios(downloadFileRequestOptions).then((response) => {
                const writer = fs.createWriteStream(filePath)

                response.data.pipe(writer);

                writer.on('finish', resolve) // not able to return bool
                writer.on('error', resolve)

            }).catch(function (thrown) {
                resolve(false);
            });
        })

    }

    /**
    * Remove this single element from the temp folder and dropbox.com
    * @param {object} element single element
    */
    removeSingleFile(element) {

        var formquery = '{"path":"' + element.path_display + '"}';
        var deleteFileRequestOptions = this.requestOptions();
        deleteFileRequestOptions.url = 'https://api.dropboxapi.com/2/files/delete';
        deleteFileRequestOptions.method = "POST";
        deleteFileRequestOptions.data = JSON.parse(formquery);

        var filePath = path.join(this.getTempFolder(), element.name);

        return new Promise((resolve, reject) => {
            axios(deleteFileRequestOptions).then((response) => {
                fs.unlink(filePath, function (err) {
                    if (err) throw err;
                    console.log('File deleted!, ' + filePath);
                    resolve();
                });
            }).catch(function (thrown) {
                resolve(false);
            });
        })
    }
}
