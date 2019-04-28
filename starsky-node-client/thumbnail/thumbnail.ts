#!/usr/bin/env node

import * as path from 'path';
import { Query } from './thumbnail-core';
require('dotenv').config({path:path.join(__dirname,"../", ".env")});
import { TaskQueue } from 'cwait';


function ShowHelpDialog() {
	console.log("Starksy Remote Thumbnail Helper")
	console.log("use numbers (e.g. 1-100) to search relative")
	console.log("use the keyword 'IMPORT' to search for recent imported files (case-sensitive)")
	console.log("use a keyword to search and check if thumbnails are created")
}


function parseArgs() {
	var args = process.argv.slice(2);
	if (args.length >= 1) {
		var parsed = parseInt(args[0])
		if (args[0] === "-h" || args[0] === "--h" || args[0] === "help") {
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

console.log("searchQuery\n", searchQuery);


query.isImportOrDirectSearch(searchQuery).then(async (fileHashList : Array<string>) => {

	process.stdout.write("∞ " + fileHashList.length + " ∞");

	// Down chain
	const queueAxios = new TaskQueue(Promise, query.MAX_SIMULTANEOUS_DOWNLOADS);
	const axiosResponses = await Promise.all(fileHashList.map(queueAxios.wrap(
		async (fileHash : string) 	=> 	{
			if(await query.checkIfSingleFileNeedsToBeDownloaded(fileHash)) {
				await query.downloadBinarySingleFile(fileHash);
				return fileHash;
			}
		}
	)));

	// Filter before send it to the up chain
	var filteredAxiosResponses : Array<string> = axiosResponses.filter(function (el) {
		return el != undefined;
	});

	process.stdout.write("% " + filteredAxiosResponses.length + " %");

	// Up chain
	const queueResizeChain = new TaskQueue(Promise, query.MAX_SIMULTANEOUS_DOWNLOADS);
	await Promise.all(filteredAxiosResponses.map(queueResizeChain.wrap(
		async (fileHash : string) 	=> 	{
			if(await query.resizeImage(fileHash)) {
				if(await query.uploadTempFile(fileHash)) {
					return fileHash; // return isn't working good
					// resizeChain> [undefined,und..]
				}
			}
		}
	)));

	
	// and clean afterwards
	query.deleteSourceTempFolder(filteredAxiosResponses);
	query.deleteTempFolder(filteredAxiosResponses);

	console.log("   `done");

}).catch( err => {
	console.log('err- downloadBinaryApiChain', err);
})


