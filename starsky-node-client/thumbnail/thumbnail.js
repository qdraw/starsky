#!/usr/bin/env node

var request = require( 'request-promise-native');
var fs = require('fs');
var path = require('path');
var jimp = require('jimp');
require('dotenv').config({path:path.join(__dirname,".env")});

var execFile = require('child_process').execFile;
var exiftool = require('dist-exiftool');

var base_url = process.env.STARKSYBASEURL;

var source_temp = "source_temp";

console.log(process.env.STARKSYBASEURL);

var options = {
    uri: base_url,
    qs: {
        f: '/2018/10/2018_10_24 Oss zonsondergang', // -> uri + '?access_token=xxxxx%20xxxxx'
		json: 'true'
    },
    headers: {
        'User-Agent': 'Request-Promise',
		'Authorization': 'Basic ' + process.env.STARKSYACCESSTOKEN,
    },
	resolveWithFullResponse: true,
    json: true // Automatically parses the JSON string in the response
};

var fileHashList = [];

function getIndexStart() {
	ensureExistsFolder(getSourceTempFolder(), 0744, function(err) {
    	if (err) console.log(err);// handle folder creation error
	});
	ensureExistsFolder(getTempFolder(), 0744, function(err) {
    	if (err) console.log(err);// handle folder creation error
	});
	request(options)
	    .then(function (items) {
			if(items.body.fileIndexItems === undefined) return;

			for (var i in items.body.fileIndexItems) {
				var item = items.body.fileIndexItems[i];
				if(item === undefined ||  item === null ||  item.fileHash.length !== 26) continue;
				if(item.imageFormat !== "jpg") continue;
				fileHashList.push(item.fileHash)
			}
			console.log("-");

			checkIfThumbnailAlreadyExist(fileHashList);
	    })
	    .catch(function (err) {
			console.log("index: " + err.response.body);
	        // API call failed...
	    });
}

getIndexStart();

function getSourceTempFolder() {
	return path.join(__dirname, source_temp);
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

function checkIfThumbnailAlreadyExist(fileHashList) {
	var ps = [];
	for (var i = 0; i < fileHashList.length; i++) { // fileHashList.length
		var read_match_details = options;
		read_match_details.uri = base_url + 'api/thumbnail/' + fileHashList[i];
		read_match_details.qs = {
			json: 'true',
			f: fileHashList[i]
		}
	    ps.push(request(read_match_details));
	}

	executeAllPromises(ps).then(function(items) {
		// Result
		var errors = items.errors.map(function(error) {
			return error.message
		}).join(',');
		var results = items.results.join(',');

		console.log(`Executed all ${ps.length} checkIfThumbnailAlreadyExist Promises:`);
		console.log(`— ${items.results.length} Promises were successful: ${results}`);
		console.log(`— ${items.errors.length} Promises failed: ${errors}`);
		var createFileHashList = createFileList(items.results);
		downloadSourceByThumb(createFileHashList);
		console.log("createFileHashList");
		console.log(createFileHashList);
	});
}

function createFileList(results) {
	console.log(results.length);
	createFileHashList = [];
	for (var i = 0; i < results.length; i++) {
		var search = results[i].request.uri.query;
		var query = JSON.parse('{"' + decodeURI(search).replace(/"/g, '\\"').replace(/&/g, '","').replace(/=/g,'":"') + '"}');
		// console.log(search);
		if(results[i].statusCode === 202) {
			createFileHashList.push(query.f)
		}
	}
	return createFileHashList;
}


function downloadSourceByThumb(fileHashList) {
	var ps = [];
	for (var i = 0; i < fileHashList.length; i++) { // fileHashList.length
		if(fileHashList[i] === undefined || fileHashList[i] === "") {
			console.log(fileHashList[i]);
			continue;
		}
		var read_match_details = options;
		read_match_details.uri = base_url + 'api/thumbnail/' + fileHashList[i];
		read_match_details.encoding = 'binary';
		read_match_details.qs = {
			f: fileHashList[i],
			issingleitem: 'true'
		}
		ps.push(request(read_match_details));
	}

	executeAllPromises(ps).then(function(items) {

		console.log(`Executed all ${ps.length} downloadSourceByThumb Promises:`);
		console.log(`— ${items.results.length} Promises were successful:`);
		console.log(`— ${items.errors.length} Promises failed:`);

		var savedItems = saveSourceByThumb(items.results);
		// add here
		resizeImage(savedItems, 0);
	});
}


function uploadThumbs(fileHashList) {
	var ps = [];
	for (var i = 0; i < fileHashList.length; i++) { // fileHashList.length
		var read_match_details = options;
		read_match_details.uri = base_url + 'import/thumbnail/' + fileHashList[i];
		read_match_details.encoding = 'binary';
		read_match_details.method = "POST";

		read_match_details.formData = {
				file: {
		            value: fs.createReadStream(path.join(getTempFolder(), fileHashList[i] + ".jpg")),
		            options: {
		                filename: fileHashList[i] + ".jpg",
		                contentType: 'image/jpg'
		            }
	        	}
			}
		read_match_details.qs = {
			f: fileHashList[i],
			issingleitem: 'true'
		}
		ps.push(request(read_match_details));
	}

	executeAllPromises(ps).then(function(items) {

		console.log(`Executed all ${ps.length} uploadThumbs Promises:`);
		console.log(`— ${items.results.length} Promises were successful:`);
		console.log(`— ${items.errors.length} Promises failed:`);
	});
}

function resizeImage(sourceFileHashesList, count) {

	if(count === undefined) count = 0;

	var sourceFilePath = path.join(getSourceTempFolder(),sourceFileHashesList[count] + ".jpg");
	var targetFilePath = path.join(__dirname,"temp",sourceFileHashesList[count] + ".jpg");

	if(sourceFileHashesList[count] === undefined) {
		console.log("sourceFileHashesList[count] === undefined");
		return;
	}
	jimp.read(sourceFilePath).then(function (lenna) {
		return lenna.resize(1000, jimp.AUTO)     // resize
			.quality(80)                 // set JPEG quality
			.write(targetFilePath); // save
	}).then(image => {
		// Do stuff with the image.
		copyExiftool(sourceFilePath, targetFilePath, sourceFileHashesList, count, function (sourceFileHashesList, count) {
			countResizeImage(sourceFileHashesList, count)
		});

	})
	.catch(function (err) {
		console.error(err);
		countResizeImage(sourceFileHashesList, count)
	});

}

function countResizeImage(sourceFileHashesList, count) {
	count++;
	if(count !== sourceFileHashesList.length) {
		resizeImage(sourceFileHashesList, count)
	}
	else {
		console.log("last");
		uploadThumbs(sourceFileHashesList);
	}
}

function copyExiftool(sourceFilePath, targetFilePath,sourceFileHashesList, count, callback) {
	execFile(exiftool, ['-overwrite_original', '-TagsFromFile', sourceFilePath, targetFilePath, '-Orientation=', ], (error, stdout, stderr) => {
	    if (error) {
	        console.error(`exec error: ${error}`);
	        return;
	    }
	    console.log(`stdout: ${stdout}`);
	    console.log(`stderr: ${stderr}`);
		return callback(sourceFileHashesList, count);
	});
}

// function checkIfNotExist(createFileHashList) {
//
// 	var checkIfNotExistList = [];
//
// 	// console.log(getTempFolder());
// 	fs.readdir(getSourceTempFolder(), function (err, files) {
// 		// "files" is an Array with files names
// 		// console.log(files);
// 		fileWithoutExtension = [];
// 		for (var i = 0; i < files.length; i++) {
// 			fileWithoutExtension.push( path.basename(files[i],".jpg"));
// 		}
//
// 		for (var i = 0; i < createFileHashList.length; i++) {
// 			// console.log(createFileHashList[i]);
// 			// console.log();
// 			if(fileWithoutExtension.indexOf(createFileHashList[i]) === -1) {
// 				checkIfNotExistList.push(createFileHashList[i]);
// 			}
// 		}
// 		downloadSourceByThumb(checkIfNotExistList);
// 		// console.log(checkIfNotExistList);
// 	});
// }

function saveSourceByThumb(results) {
	// console.log(results.length);
	createFileHashList = [];
	for (var i = 0; i < results.length; i++) {
		var search = results[i].request.uri.query;
		var query = JSON.parse('{"' + decodeURI(search).replace(/"/g, '\\"').replace(/&/g, '","').replace(/=/g,'":"') + '"}');
		console.log(results[i].statusCode);
		if(results[i].statusCode === 200) {

			console.log(query.f);
			var filePath = path.join(getSourceTempFolder(),query.f + ".jpg");
			createFileHashList.push(query.f);

			// sync >>>
			fs.writeFileSync(filePath, results[i].body, 'binary');
		}

	}
	return createFileHashList;
}

// "2BH2TMPJDYERIM6ZRMOASCYCWE","5OB5MWJU2GEID5MZO653AYHD7A",
// uploadThumbs(["H2PIVQDAKAN3PGMD3R7TOMGEKU","H2PIVQDAKAN3PGMD3R7TOMGEK1"]);



function executeAllPromises(promises) {
  // Wrap all Promises in a Promise that will always "resolve"
  var resolvingPromises = promises.map(function(promise) {
    return new Promise(function(resolve) {
      var payload = new Array(2);
      promise.then(function(result) {
          payload[0] = result;
        })
        .catch(function(error) {
          payload[1] = error;
        })
        .then(function() {
          /*
           * The wrapped Promise returns an array:
           * The first position in the array holds the result (if any)
           * The second position in the array holds the error (if any)
           */
          resolve(payload);
        });
    });
  });

  var errors = [];
  var results = [];

  // Execute all wrapped Promises
  return Promise.all(resolvingPromises)
    .then(function(items) {
      items.forEach(function(payload) {
        if (payload[1]) {
          errors.push(payload[1]);
        } else {
          results.push(payload[0]);
        }
      });

      return {
        errors: errors,
        results: results
      };
    });
}
