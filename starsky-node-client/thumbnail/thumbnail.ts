#!/usr/bin/env node

var path = require('path');

import { Query } from './thumbnail-core';
// import { OptionsWithUri } from 'request-promise-native';
import { IResults } from './IResults';
require('dotenv').config({path:path.join(__dirname,".env")});




function parseArgs() {
	var args = process.argv.slice(2);
	if (args.length >= 1) {
		var parsed = parseInt(args[0])
		if (isNaN(parsed)) {
			return args[0];
		}
		else if(parsed === 0 ){
			// Search for today
			return "-Datetime>0 -ImageFormat:jpg -!delete";
		}
		else {
			// 1 = yesterday
			return "-Datetime>" + parsed  + " -Datetime<"+ (parsed - 1) + " -ImageFormat:jpg -!delete";
		}
	}

	var parsedDefault = 1;
	return "-Datetime>" + (parsedDefault)  + " -ImageFormat:jpg -!delete";
}

var searchQuery = parseArgs();

var base_url = process.env.STARKSYBASEURL;
var access_token = process.env.STARKSYACCESSTOKEN;

var query = new Query(base_url,access_token);

var indexRequestOptions = query.indexRequestOptions(searchQuery);


query.searchIndex(indexRequestOptions).then((result : IResults )  => {
	return result;
}).catch( err => {
	console.log('err', err);
}).then((result : IResults) => {
	// console.log('searchIndex => ', result);
	return query.checkIfSingleFileNeedsToBeDownloadedApiChain(result.fileHashList);
}).catch( err => {
	console.log('err- checkIfSingleFileNeedsToBeDownloadedApiChain', err);
}).then((result : Array<string>) => {
	return query.downloadBinaryApiChain(result);
}).catch( err => {
	console.log('err- downloadBinaryApiChain', err);
}).then((result : Array<string>) => {
	console.log(result.length)
	return query.resizeChain(result);
}).catch( err => {
	console.log('err- resizeChain', err);
}).then((result : Array<string>) => {
	console.log(result.length)
	return query.uploadTempFileChain(result);
}).catch( err => {
	console.log('err- uploadTempFileChain', err);
}).then((result : Array<string>) => {
	return query.deleteFileChain(result);
}).catch( err => {
	console.log('err- deleteFileChain', err);
});

