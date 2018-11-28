#!/usr/bin/env node

var request = require( 'request-promise-native');
var fs = require('fs');
var path = require('path');
var jimp = require('jimp');
require('dotenv').config({path:path.join(__dirname,".env")});

var execFile = require('child_process').execFile;
var exiftool = require('dist-exiftool');

var base_url = process.env.STARKSYBASEURL;

var requestOptions = {
    uri: base_url,
	method: "GET",
    headers: {
        'User-Agent': 'MS FrontPage Express',
		'Authorization': 'Basic ' + process.env.STARKSYACCESSTOKEN,
    },
	resolveWithFullResponse: true,
    json: true // Automatically parses the JSON string in the response
};

getSubPathRelative(-1);


function getSubPathRelative(subpathRelativeValue) {
	var subPathRelativeRequestOptions = requestOptions;
	subPathRelativeRequestOptions.uri = base_url + 'Redirect/SubpathRelative/';
	subPathRelativeRequestOptions.qs = {
        value: subpathRelativeValue,
		json: 'true'
    };

	request(subPathRelativeRequestOptions)
	    .then(function (items) {
			console.log(items.body);
			getIndexStart(items.body);
		})
	    .catch(function (err) {
			console.log(err);
			console.log("getSubPathRelative: " + err.response.body);
	        // API call failed...
	    });
}

function getIndexStart(subpath) {
	ensureExistsFolder(getSourceTempFolder(), 0744, function(err) {
    	if (err) console.log(err);// handle folder creation error
	});
	ensureExistsFolder(getTempFolder(), 0744, function(err) {
    	if (err) console.log(err);// handle folder creation error
	});


	var indexRequestOptions = requestOptions;
	indexRequestOptions.uri = base_url;
	indexRequestOptions.qs = {
		f: subpath,
		json: 'true'
	};

	request(requestOptions)
	    .then(function (items) {
			if(items.body.fileIndexItems === undefined) return;

			var fileHashList = [];
			for (var i in items.body.fileIndexItems) {
				var item = items.body.fileIndexItems[i];
				if(item === undefined ||  item === null ||  item.fileHash.length !== 26) continue;
				if(item.imageFormat !== "jpg") continue;
				fileHashList.push(item.fileHash)
			}
			console.log("-");
			downloadSourceTempFile(fileHashList,0);

	    })
	    .catch(function (err) {
			console.log(err);
			console.log("index: " + err.response.body);
	        // API call failed...
	    });
}

function downloadSourceTempFile(sourceFileHashesList,i) {
	var downloadrequestOptions = requestOptions;
	downloadrequestOptions.uri = base_url + 'api/thumbnail/' + sourceFileHashesList[i];
	downloadrequestOptions.method = "GET";
	downloadrequestOptions.encoding = 'UTF-8';
	delete downloadrequestOptions.formData;

	downloadrequestOptions.qs = {
		json: 'true',
		f: sourceFileHashesList[i]
	}


	request(downloadrequestOptions)
	    .then(function (result) {
			if(result.statusCode === 202) {
				console.log(result.statusCode, i, sourceFileHashesList.length, sourceFileHashesList[i]);
				chain(sourceFileHashesList, i, downloadSourceTempFile, done)
			}
			else {
				console.log(result.statusCode, i, sourceFileHashesList.length, sourceFileHashesList[i]);
				next(sourceFileHashesList, i, downloadSourceTempFile, done)
			}
		})
	    .catch(function (err) {
			console.log("downloadrequestOptions - " + sourceFileHashesList[i]);
			console.log(err.message);
			next(sourceFileHashesList, i, downloadSourceTempFile, done)
	    });
}

function done() {
	console.log("DSf");
}

function chain(sourceFileHashesList, i, callback, finalCallback) {

	var downloadFilerequestOptions = requestOptions;
	downloadFilerequestOptions.uri = base_url + 'api/thumbnail/' + sourceFileHashesList[i];
	downloadFilerequestOptions.encoding = 'binary';
	downloadFilerequestOptions.method = "GET";
	downloadFilerequestOptions.qs = {
		f: sourceFileHashesList[i],
		issingleitem: 'true'
	}

	request(downloadFilerequestOptions)
		.then(function (fileResults) {
			var filePath = path.join(getSourceTempFolder(),sourceFileHashesList[i] + ".jpg");

			fs.writeFile(filePath, fileResults.body, 'binary', function (res) {
				resizeImage(sourceFileHashesList[i],function (fileHash) {
					uploadTempFile(sourceFileHashesList, i, callback, finalCallback);
				})
			});
			// next(sourceFileHashesList, i, callback, finalCallback)

		})
		.catch(function (err) {
			console.log("downloadFilerequestOptions");
			console.log(err);
			next(sourceFileHashesList, i, callback, finalCallback)
		});
}

function next(sourceFileHashesList, count, callback, finalCallback) {
	deleteFile(sourceFileHashesList, count);

	count++;
	if(count < sourceFileHashesList.length) {
		callback(sourceFileHashesList, count, callback, finalCallback)
	}
	else {
		console.log("last");
		finalCallback(sourceFileHashesList);
	}
}

function uploadTempFile(sourceFileHashesList, i,callback, finalCallback) {
	var uploadRequestOptions = requestOptions;
	var fileHash = sourceFileHashesList[i];
	uploadRequestOptions.uri = base_url + 'import/thumbnail/' + fileHash;
	uploadRequestOptions.encoding = 'binary';
	uploadRequestOptions.method = "POST";


	uploadRequestOptions.formData = {
			file: {
				value: fs.createReadStream(path.join(getTempFolder(), fileHash + ".jpg")),
				options: {
					filename: fileHash + ".jpg",
					contentType: 'image/jpg'
				}
			}
		}
	uploadRequestOptions.qs = {
		f: fileHash,
		issingleitem: 'true'
	}
	request(uploadRequestOptions)
		.then(function (uploadResults) {
			console.log(uploadResults.body);
			next(sourceFileHashesList, i, callback, finalCallback);
		})
		.catch(function (err) {
			console.log("uploadRequestOptions");
			console.log(err);
		});
}

function deleteFile(sourceFileHashesList, i) {
	var file1 = path.join(getTempFolder(), sourceFileHashesList[i] + ".jpg");
	fs.access(file1, fs.constants.F_OK, (err) => {
		if(err) return;
		fs.unlink(file1,function(err){
			if(err) return console.log(err);
		});
	});

	var file2 = path.join(getSourceTempFolder(), sourceFileHashesList[i] + ".jpg");
	fs.access(file2, fs.constants.F_OK, (err) => {
		if(err) return;
		fs.unlink(file2,function(err){
			if(err) return console.log(err);
		});
	});
}




function resizeImage(fileHash,callback) {

	if(fileHash === undefined) {
		console.log("fileHash === undefined");
		return;
	}
	var sourceFilePath = path.join(getSourceTempFolder(),fileHash + ".jpg");
	var targetFilePath = path.join(getTempFolder(),fileHash + ".jpg");


	jimp.read(sourceFilePath).then(function (lenna) {
		return lenna.resize(1000, jimp.AUTO)     // resize
			.quality(80)                 // set JPEG quality
			.write(targetFilePath); // save
		}).then(image => {
			// Do stuff with the image.
			copyExiftool(sourceFilePath, targetFilePath, fileHash, function (fileHash) {
				callback(fileHash)
		});

	})
	.catch(function (err) {
		console.error(err);
	});

}



function copyExiftool(sourceFilePath, targetFilePath,fileHash, callback) {
	execFile(exiftool, ['-overwrite_original', '-TagsFromFile', sourceFilePath, targetFilePath, '-Orientation=', ], (error, stdout, stderr) => {
	    if (error) {
	        console.error(`exec error: ${error}`);
	        return;
	    }
	    // console.log(`stdout: ${stdout}`);
		if(stderr !== "") console.log(`stderr: ${stderr}`);
		return callback(fileHash);
	});
}


function getSourceTempFolder() {
	return path.join(__dirname, "source_temp");
}
function getTempFolder() {
	return path.join(__dirname, "temp");
}


function ensureExistsFolder(path, mask, cb) {
    if (typeof mask == 'function') { // allow the `mask` parameter to be optional
        cb = mask;
        mask = 0777;
    }
    fs.mkdir(path, mask, function(err) {
        if (err) {
            if (err.code == 'EEXIST') cb(null); // ignore the error if the folder already exists
            else cb(err); // something else went wrong
        } else cb(null); // successfully created folder
    });
}
