var path = require('path');
var fs = require('fs');
const axios = require('axios');


module.exports = class Dropbox {
	access_token;

	constructor(access_token) {
		this.access_token = access_token;

		// Make sure the output directories exist
		this.getRights();
    }

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
    
    getTempFolder() {
		return path.join(__dirname, "temp");
	}

    getRights() {
		this.ensureExistsFolder(this.getTempFolder(), parseInt('0744', 8), function (err) {
			if (err) console.log(err);// handle folder creation error
		});
	}
    
    requestOptions(contentLength) {
		return {
			url: this.base_url,
			method: "GET",
			headers: {
				'User-Agent': 'MS FrontPage Express',
                'Authorization': 'Bearer ' + this.access_token,
                'Content-Length': contentLength,
                'Content-Type': 'application/json'
			},
		}
    };
    
    listFiles(dropboxfolder) {
        this.getRights();
        return new Promise((resolve, reject) => {
            (async () => {
                var response = await this.listQuery(dropboxfolder);
                if(!response.data.has_more) {
                    resolve();
                }

                while (response.data.has_more) {
                    response = await this.listQueryCursor(response);
                }
                resolve(response.data.entries);
            })()
        });
    }

    listQueryCursor(inputResponse) {
        var cursorQuery = '{"cursor":"' + inputResponse.data.cursor +'"}';
        var listQueryRequestOptions = this.requestOptions(cursorQuery.length);
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

    listQuery(dropboxfolder) {
        var formquery = '{"path":"' + dropboxfolder +'"}';
        var listQueryRequestOptions = this.requestOptions(formquery.length);
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

    downloadBinarySingleFile(hashItem) {

        this.getRights();

        var downloadFileRequestOptions = this.requestOptions();
        downloadFileRequestOptions.url = this.base_url + 'api/thumbnail/' + hashItem;
        downloadFileRequestOptions.responseType = 'stream'
        downloadFileRequestOptions.method = "GET";
        downloadFileRequestOptions.params = {
            f: hashItem,
            issingleitem: 'true'
        }

        var filePath = path.join(this.getSourceTempFolder(), hashItem + ".jpg");

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
}
