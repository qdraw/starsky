var core = require('./thumbnail.core');
var request = require( 'request-promise-native');

var importRequestOptions = core.requestOptions;
importRequestOptions.uri = process.env.STARKSYBASEURL + 'import/history/';

request(importRequestOptions)
	.then(function (items) {
		console.log(items.body.length);

		if(items.body.length >= 1) {
			var searchQueryList = [];
			var searchQuery = "";
			for (var i in items.body) {
				var item = items.body[i];
				if(item === undefined ||  item === null ||  item.fileHash.length !== 26) continue;
				searchQueryList.push(item.dateTime);
				searchQuery += " -Datetime=" + searchQueryList[i] + " ||";
			}

			core.getSearchStart(searchQuery,0);

			// core.downloadSourceTempFile(fileHashList,0,"historyAPI");
		}
	})
	.catch(function (err) {
		console.log(err);
		console.log("index: " + err.response.body);
		// API call failed...
	});
