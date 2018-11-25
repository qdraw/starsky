#!/usr/bin/env node

var request = require( 'request-promise-native');
const fs = require('fs');
var base_url = 'https://?.qdraw.eu/';

var options = {
    uri: base_url,
    qs: {
        f: '/2018/11/', // -> uri + '?access_token=xxxxx%20xxxxx'
		json: 'true'
    },
    headers: {
        'User-Agent': 'Request-Promise',
		'Authorization': 'Basic ?='
    },
	resolveWithFullResponse: true,
    json: true // Automatically parses the JSON string in the response
};

var fileHashList = [];

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
		createFileList(items.results)
	});

	// Promise.all(ps)
	//     .then((results) => {
	// 		createFileList(results)
	//     }).catch(
	// 		err => {
	// 			console.log();
	// 			// write to file
	// 			fs.writeFile ("input.json", JSON.stringify(err), function(err) {
	// 			    if (err) throw err;
	// 			    console.log('complete');
	// 			    }
	// 			);
	//
	// 			createFileList(err)
	// 		}
	// 	);
}

function createFileList(results) {
	console.log(results.length);
	createFileHashList = [];
	for (var i = 0; i < results.length; i++) {
		var search = results[i].request.uri.query;
		var query = JSON.parse('{"' + decodeURI(search).replace(/"/g, '\\"').replace(/&/g, '","').replace(/=/g,'":"') + '"}');
		console.log(search);
		if(results[i].statusCode === 202) {
			createFileHashList.push(query.f)
		}
	}
	console.log(createFileHashList);
}





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

// var myPromises = [
//   Promise.resolve(1),
//   Promise.resolve(2),
//   Promise.reject(new Error('3')),
//   Promise.resolve(4),
//   Promise.reject(new Error('5'))
// ];

// executeAllPromises(myPromises).then(function(items) {
// 	// Result
// 	var errors = items.errors.map(function(error) {
// 		return error.message
// 	}).join(',');
// 	var results = items.results.join(',');
//
// 	console.log(`Executed all ${myPromises.length} Promises:`);
// 	console.log(`— ${items.results.length} Promises were successful: ${results}`);
// 	console.log(`— ${items.errors.length} Promises failed: ${errors}`);
// });
