#!/usr/bin/env node

var request = require( 'request-promise-native');
var fs = require('fs');
var path = require('path');
var jimp = require('jimp');

var base_url = 'https://~.qdraw.eu/';

var source_temp = "source_temp";

var options = {
    uri: base_url,
    qs: {
        f: '/2018/11/2018_11_24', // -> uri + '?access_token=xxxxx%20xxxxx'
		json: 'true'
    },
    headers: {
        'User-Agent': 'Request-Promise',
		'Authorization': 'Basic ~='
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

			checkIfThumbnailAlreadyExist(fileHashList);
	    })
	    .catch(function (err) {
	        // API call failed...
	    });
}

// getIndexStart();

function getSourceTempFolder() {
	return path.join(__dirname, source_temp);
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

		console.log(`Executed all ${ps.length} Promises:`);
		console.log(`— ${items.results.length} Promises were successful: ${results}`);
		console.log(`— ${items.errors.length} Promises failed: ${errors}`);
		var createFileHashList = createFileList(items.results);

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

		console.log(`Executed all ${ps.length} Promises:`);
		console.log(`— ${items.results.length} Promises were successful:`);
		console.log(`— ${items.errors.length} Promises failed:`);

		var savedItems = saveSourceByThumb(items.results);

	});
}

function resizeImage(sourceFileHashesList) {
	for (var i = 0; i < sourceFileHashesList.length; i++) {

		var filePath = path.join(getSourceTempFolder(),sourceFileHashesList[i] + ".jpg");
		console.log(filePath);
		jimp.read(filePath).then(function (lenna) {
			return lenna.resize(1000, jimp.AUTO)     // resize
				.quality(80)                 // set JPEG quality
				.write(path.join(__dirname,"temp",sourceFileHashesList[i] + "_kl.jpg")); // save
		})
		.catch(function (err) {
			console.error(err);
		});
	}



}

function checkIfNotExist(createFileHashList) {

	var checkIfNotExistList = [];

	// console.log(getTempFolder());
	fs.readdir(getSourceTempFolder(), function (err, files) {
		// "files" is an Array with files names
		// console.log(files);
		fileWithoutExtension = [];
		for (var i = 0; i < files.length; i++) {
			fileWithoutExtension.push( path.basename(files[i],".jpg"));
		}

		for (var i = 0; i < createFileHashList.length; i++) {
			// console.log(createFileHashList[i]);
			// console.log();
			if(fileWithoutExtension.indexOf(createFileHashList[i]) === -1) {
				checkIfNotExistList.push(createFileHashList[i]);
			}
		}
		downloadSourceByThumb(checkIfNotExistList);
		// console.log(checkIfNotExistList);
	});
}

function saveSourceByThumb(results) {
	// console.log(results.length);
	createFileHashList = [];
	for (var i = 0; i < results.length; i++) {
		var search = results[i].request.uri.query;
		var query = JSON.parse('{"' + decodeURI(search).replace(/"/g, '\\"').replace(/&/g, '","').replace(/=/g,'":"') + '"}');
		console.log(results[i].statusCode);
		if(results[i].statusCode === 200) {

			var filePath = path.join(getSourceTempFolder(),query.f + ".jpg");
			createFileHashList.push(query.f);

			// sync >>>
			fs.writeFileSync(filePath, results[i].body, 'binary');
		}

	}
	return createFileHashList;
}

// "2BH2TMPJDYERIM6ZRMOASCYCWE","5OB5MWJU2GEID5MZO653AYHD7A",
resizeImage(["H2PIVQDAKAN3PGMD3R7TOMGEKU"]);



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
