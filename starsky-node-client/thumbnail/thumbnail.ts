#!/usr/bin/env node

var path = require('path');

import { Query } from './thumbnail-core';
import { IResults } from './IResults';
require('dotenv').config({path:path.join(__dirname,"../", ".env")});
import { TaskQueue } from 'cwait';

import {map as pMap } from 'p-iteration';

function ShowHelpDialog() {
	console.log("Starksy Remote Thumbnail Helper")
	console.log("use numbers (e.g. 1-100) to search relative")
	console.log("use the keyword 'import' to search for recent imported files")
	console.log("use a keyword to search and check if thumbnails are created")
}


function parseArgs() {
	var args = process.argv.slice(2);
	if (args.length >= 1) {
		var parsed = parseInt(args[0])
		if (args[0] === "-h" || args[0] === "--h") {
			ShowHelpDialog();
		}
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


var skip = function() {
	console.log("skip");
};

if(searchQuery === "IMPORT") {

	query.isImportIndex().then(async (filePathList : Array<string>) => {


		await query.searchIndexList(filePathList);

		// var requestQueue = filePathList.map(file => query.searchIndex(query.indexRequestOptions("-filePath:" + file)));
		// console.log(requestQueue);

		// requestQueue.reduce((curr, next) => {
		// 	console.log(curr)
		// 	return curr.then(() => next); // <- here
		//   }, Promise.resolve())
		// 	  .then((res) => console.log(res));



		// console.log(filePathList.length);
		// const finalResult = await query.searchIndex(indexRequestOptions);

		// filePathList.forEach(async filePath => {
		// 	var indexRequestOptions = query.indexRequestOptions("-filePath:" + filePath);
		// 	console.log(finalResult);
		// });




		// var combineFileHashGetPromises = Array<Promise<IResults>>();
		// filePathList.forEach(async filePath => {
		// 	var indexRequestOptions = query.indexRequestOptions("-filePath:" + filePath);

		// 	combineFileHashGetPromises.push(query.searchIndex);
		// });
		

		// var searchQueryWithFilePathList = Array<string>();
		// filePathList.forEach(element => {
		// 	searchQueryWithFilePathList.push(element);
		// });

		// pMap(filePathList, async filePath => {
		// 	var indexRequestOptions = query.indexRequestOptions("-filePath:" + filePath);
		// 	await query.searchIndex(indexRequestOptions);
		// });

		// combineFileHashGetPromises.reduce(function(cur, next){
		// 	return cur('').then(skip, next);
		// }, Promise.reject()).then(function(){
		// 	console.log("done");
		// }, function() {
		// 	console.log("failed");
		// });
	
	}).catch( err => {
		console.log('err- deleteFileChain', err);
	})

}





// query.isImportIndex().then((searchQueryResult : string) => {
// 	var indexRequestOptions = query.indexRequestOptions(searchQueryResult);
// 	return query.searchIndex(indexRequestOptions);
// }).catch( err => {
// 	console.log('err- deleteFileChain', err);
// }).then((result : IResults) => {
// 	// console.log('searchIndex => ', result);
// 	return query.checkIfSingleFileNeedsToBeDownloadedApiChain(result.fileHashList);
// }).catch( err => {
// 	console.log('err- checkIfSingleFileNeedsToBeDownloadedApiChain', err);
// })

// .then((result : Array<string>) => {
// 	return query.downloadBinaryApiChain(result);
// }).catch( err => {
// 	console.log('err- downloadBinaryApiChain', err);
// }).then((result : Array<string>) => {
// 	// console.log(result.length)
// 	return query.resizeChain(result);
// }).catch( err => {
// 	console.log('err- resizeChain', err);
// }).then((result : Array<string>) => {
// 	// console.log(result.length)
// 	return query.uploadTempFileChain(result);
// }).catch( err => {
// 	console.log('err- uploadTempFileChain', err);
// }).then((result : Array<string>) => {
// 	return query.deleteFileChain(result);
// }).catch( err => {
// 	console.log('err- deleteFileChain', err);
// });
