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

console.log("searchQuery", searchQuery);

query.isImportOrDirectSearch(searchQuery).then(async (fileHashList : Array<string>) => {

	// Down chain
	const queueAxios = new TaskQueue(Promise, query.MAX_SIMULTANEOUS_DOWNLOADS);
	const axiosResponses = await Promise.all(fileHashList.map(queueAxios.wrap(
		async (fileHash : string) 	=> 	{
			if(await query.checkIfSingleFileNeedsToBeDownloaded(fileHash)) {
				if(await query.downloadBinarySingleFile(fileHash)) {
					return fileHash;
				}
			}
		}
	)));

	process.stdout.write("%");

	// Up chain
	const queueResizeChain = new TaskQueue(Promise, query.MAX_SIMULTANEOUS_DOWNLOADS);
	const resizeChain = await Promise.all(axiosResponses.map(queueResizeChain.wrap(
		async (fileHash : string) 	=> 	{
			if(await query.resizeImage(fileHash)) {
				return fileHash;
			}
		}
	)));

	console.log('resizeChain', resizeChain);

}).catch( err => {
	console.log('err- downloadBinaryApiChain', err);
})



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
