#!/usr/bin/env node

import { TaskQueue } from 'cwait';
import * as path from 'path';
import { Files, Parser, Query } from './thumbnail-core';
require('dotenv').config({ path: path.join(__dirname, "../", ".env") });

var base_url = process.env.STARKSYBASEURL;
var access_token = process.env.STARKSYACCESSTOKEN;

if (!base_url || !access_token) {
	throw new Error("Missing env's STARKSYBASEURL or STARKSYACCESSTOKEN")
}

// Cleanup old files
new Files().RemoveOldFiles();

function ShowHelpDialog() {
	console.log("Starksy Remote Thumbnail Helper")
	console.log("use numbers (e.g. 1) to search relative")
	console.log("use a range to search relative in that range (e.g. 1-7 to search for last week)")
	console.log("use the keyword 'IMPORT' to search for recent imported files (case-sensitive)")
	console.log("use a keyword to search and check if thumbnails are created")
}

function parseArgs(): string[] {
	var args = process.argv.slice(2);
	if (args.length >= 1) {
		var parsedInt = parseInt(args[0])
		var isRange = args[0].indexOf("-") >= 1;

		if (args[0] === "-h" || args[0] === "--h" || args[0] === "help") {
			ShowHelpDialog();
		}
		else if (isRange && args[0].split("-").length === 2) {
			return new Parser().parseRanges(args);
		}
		if (isNaN(parsedInt)) {
			return [args[0]];
		}
		else if (parsedInt === 0) {
			// Search for today
			return ["-Datetime>0 -ImageFormat:jpg -!delete"];
		}
		else {
			// 1 = yesterday
			return ["-Datetime>" + parsedInt + " -Datetime<" + (parsedInt - 1) + " -ImageFormat:jpg -!delete"];
		}
	}
	var parsedDefault = 1;
	return ["-Datetime>" + (parsedDefault) + " -ImageFormat:jpg -!delete"];
}

var query = new Query(base_url, access_token);

runQueryChain(0, parseArgs());

function runQueryChain(index = 0, searchQueries: string[]) {
	if (searchQueries.length === 0) return;
	if (index >= searchQueries.length) return;

	console.log(searchQueries[index] + "\n^^^^searchQuery^^^^");

	query.isImportOrDirectSearch(searchQueries[index]).then(async (fileHashList: Array<string>) => {
		process.stdout.write("∞ " + fileHashList.length + " ∞");

		// Down chain
		const queueAxios = new TaskQueue(Promise, query.MAX_SIMULTANEOUS_DOWNLOADS);
		const axiosResponses = await Promise.all(fileHashList.map(queueAxios.wrap(
			async (fileHash: string) => {
				if (await query.checkIfSingleFileNeedsToBeDownloaded(fileHash)) {
					return fileHash;
				}
			}
		)));

		// Filter before send it to the up chain
		var filteredAxiosResponses: Array<string> = axiosResponses.filter(function (el) {
			return el != undefined;
		});

		process.stdout.write("% " + filteredAxiosResponses.length + " %");

		// Up chain
		const queueResizeChain = new TaskQueue(Promise, query.MAX_SIMULTANEOUS_DOWNLOADS);
		await Promise.all(filteredAxiosResponses.map(queueResizeChain.wrap(
			async (fileHash: string) => {
				await query.downloadBinarySingleFile(fileHash);
				if (await query.resizeImage(fileHash)) {
					if (await query.uploadTempFile(fileHash)) {
						return fileHash; // return isn't working good
						// resizeChain> [undefined,und..]
					}
				}
				else {
					// continue if a single file fails e.g. RangeError: Array buffer allocation failed
					process.stdout.write('>>> image has failed: ' + fileHash);
					return fileHash;
				}
			}
		)));

		// and clean afterwards
		query.deleteSourceTempFolder(filteredAxiosResponses);
		query.deleteTempFolder(filteredAxiosResponses);

		console.log("   `done " + index + "/" + (searchQueries.length - 1));

		// Next
		index++;
		runQueryChain(index, searchQueries);

	}).catch(err => {
		console.log('err- downloadBinaryApiChain', err);
	})

}