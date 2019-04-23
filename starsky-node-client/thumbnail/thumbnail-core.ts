
var path = require('path');
var fs = require('fs');

var Jimp = require('jimp'); //es6 -> fails

import { OptionsWithUri, FullResponse } from "request-promise-native";

import request = require('request-promise-native');

import { IResults } from "./IResults";
import { resolve } from 'path';
var execFile = require('child_process').execFile;
var exiftool = require('dist-exiftool');



	
export class Query {

	base_url: string;
	access_token: string;


	constructor(base_url: string, access_token : string) {		
		this.base_url = base_url;
		this.access_token = access_token;
	}

	private resolveSequentially(arr) {
		let r = Promise.resolve();
		return Promise.all(
		  arr.reduce((p, c) => {
			r = r.then(() => (typeof c === 'function' ? c() : c));
			p.push(r);
			return p;
		  }, []),
		);
	}

	public requestOptions() : OptionsWithUri {
		return {
			uri: this.base_url,
			method: "GET",
			headers: {
				'User-Agent': 'MS FrontPage Express',
				'Authorization': 'Basic ' + this.access_token,
			},
			resolveWithFullResponse: true,
			json: true // Automatically parses the JSON string in the response
		}
	};

	public indexRequestOptions(searchQuery: string, pageNumber = 0) : OptionsWithUri {
	
		var indexRequestOptions : OptionsWithUri = this.requestOptions();
		indexRequestOptions.uri = this.base_url + "search";
		indexRequestOptions.qs = {
			t: searchQuery,
			json: 'true',
			p: pageNumber
		};
		return indexRequestOptions;
	}


	private ensureExistsFolder(path, mask, cb) {
		if (typeof mask == 'function') { // allow the `mask` parameter to be optional
			cb = mask;
			mask = parseInt('0777',8);
		}
		fs.mkdir(path, mask, function(err) {
			if (err) {
				if (err.code == 'EEXIST') cb(null); // ignore the error if the folder already exists
				else cb(err); // something else went wrong
			} else cb(null); // successfully created folder
		});
	}

	private getSourceTempFolder() {
		return path.join(__dirname, "source_temp");
	}

	private getTempFolder() {
		return path.join(__dirname, "temp");
	}

	private getRights() {
		this.ensureExistsFolder(this.getSourceTempFolder(), parseInt('0744',8) , function(err) {
			if (err) console.log(err);// handle folder creation error
		});
		this.ensureExistsFolder(this.getTempFolder(), parseInt('0744',8), function(err) {
			if (err) console.log(err);// handle folder creation error
		});
	}

	public parseFileIndexItems(items : FullResponse) : Array<string>{

		var fileHashList = new Array<string>();
		for (var i in items.body.fileIndexItems) {

			var item = items.body.fileIndexItems[i];
			if(item === undefined ||  item === null ||  item.fileHash.length !== 26) continue;
			if(item.imageFormat !== "jpg") continue;
			fileHashList.push(item.fileHash)
		}
		return fileHashList;
	}


	// public async processResult(sourceFileHashesList : Array<string>): Promise<Array<string>> {

	// }



	public async searchIndex(indexRequestOptions : OptionsWithUri): Promise<IResults> {

		this.getRights();

		var those = this;

		return new Promise<IResults>((resolve, reject) => {

			request(indexRequestOptions)
				.then(function (items : FullResponse) {
					
					if(items.body.fileIndexItems === undefined) reject();
		
					var searchQuery = items.body.searchQuery

					var fileHashList = those.parseFileIndexItems(items);
					
		
					if(fileHashList.length === 0) console.log("> 0 imageFormat:jpg items");
		
					var lastPageNumber = items.body.lastPageNumber;

					if(fileHashList.length <= 0) {
						reject(<IResults>{
							fileHashList : new Array<string>()
						});
					}
					else {
						var result = <IResults>{
							fileHashList,
							lastPageNumber,
							searchQuery
						};

						var requests = [];
						for (var i = 1; i <= lastPageNumber; i++) {
							var indexRequestOptions = those.indexRequestOptions(searchQuery,i);
							requests.push(request(indexRequestOptions));
						}

						// All at one
						Promise.all(requests).then(function (items) {

							items.forEach(item => {
								
								if(item.body.fileIndexItems === undefined) reject();
								var fileHashList = those.parseFileIndexItems(item);

								fileHashList.forEach(element => {
									result.fileHashList.push(element);
								});
							});

							resolve(result);

						})
						.catch(function (err) {
							reject();
						});
						
					}
				})
				.catch(function (err) {
					console.log(err);
					console.log("index: " + err.response.body);
					reject(<IResults>{});
					// API call failed...
				});

		});


		
	}

	public async checkIfSingleFileNeedsToBeDownloadedApiChain(sourceFileHashesList : Array<string>): Promise<Array<string>> {


		var downloadPromises = [];
		sourceFileHashesList.forEach(hashItem => {
			downloadPromises.push(this.checkIfSingleFileNeedsToBeDownloaded(hashItem));
		});

		return new Promise<Array<string>>((resolve, reject) => {

			if(sourceFileHashesList.length === 0) reject([]);

			this.resolveSequentially(downloadPromises).then(( result : Array<any>) => { 

				var filtered : Array<string> = result.filter(function (el) {
					return el != null;
				});

				resolve(filtered);
			})
		});
			
	}

	public async checkIfSingleFileNeedsToBeDownloaded(hashItem : string): Promise<string> {

		var downloadFileRequestOptions = this.requestOptions();;
		downloadFileRequestOptions.uri = this.base_url + 'api/thumbnail/' + hashItem;
		downloadFileRequestOptions.method = "GET";
		downloadFileRequestOptions.encoding = 'UTF-8';
		delete downloadFileRequestOptions.formData;

		downloadFileRequestOptions.qs = {
			json: 'true',
			f: hashItem
		}

		return new Promise<string>((resolve, reject) => {

			request(downloadFileRequestOptions)
				.then(function (result : FullResponse) {
					
					if(result.statusCode === 202) {
						console.log(hashItem, result.statusCode);
						resolve(hashItem)
					}
					else {
						resolve(null);
					}
				})
				.catch(function (err) {
					console.log(err.statusCode, "checkIfSingleFileNeedsToBeDownloaded");
					resolve(null);
				});
		});
	}


	public async downloadBinarySingleFile(hashItem : string): Promise<string> {

		var those = this; 
		var downloadFileRequestOptions = this.requestOptions();
		downloadFileRequestOptions.uri = this.base_url + 'api/thumbnail/' + hashItem;
		downloadFileRequestOptions.encoding = 'binary';
		downloadFileRequestOptions.method = "GET";
		downloadFileRequestOptions.qs = {
			f: hashItem,
			issingleitem: 'true'
		}
		return new Promise<string>((resolve, reject) => {

			request(downloadFileRequestOptions)
				.then(function (fileResults) {
					var filePath = path.join(those.getSourceTempFolder(), hashItem + ".jpg");
					fs.writeFile(filePath, fileResults.body, 'binary', function (res) {
						resolve(hashItem);
					});
				})
				.catch(function (err) {
					console.log("downloadBinarySingleFile");
					console.log(err.statusCode);

					reject();

				});
		});

	}


	public async downloadBinaryApiChain(sourceFileHashesList : Array<string>): Promise<Array<string>> {

		var downloadPromises = [];
		sourceFileHashesList.forEach(hashItem => {
			if(hashItem != null ) {
				downloadPromises.push(this.downloadBinarySingleFile(hashItem));
			};
		});

		console.log('downloadBinaryApiChain sourceFileHashesList', sourceFileHashesList)

		return new Promise<Array<string>>((resolve, reject) => {
			this.resolveSequentially(downloadPromises).then(( result : Array<any>) => { 
				resolve(result);
			})
		});
	}



	public async resizeChain(sourceFileHashesList : Array<string>): Promise<Array<string>> {

		var resizePromises = [];
		sourceFileHashesList.forEach(hashItem => {
			resizePromises.push(this.resizeImage(hashItem));
		});

		return new Promise<Array<string>>((resolve, reject) => {
			this.resolveSequentially(resizePromises).then(( result : Array<any>) => { 
				resolve(result);
			})
		});

	}

	public async resizeImage(fileHash : string): Promise<string> {

		var sourceFilePath = path.join(this.getSourceTempFolder(),fileHash + ".jpg");
		var targetFilePath = path.join(this.getTempFolder(),fileHash + ".jpg");

		return new Promise<string>((resolve, reject) => {

			if(fileHash === undefined) {
				console.log("fileHash === undefined");
				reject();
			}

			Jimp.read(sourceFilePath)
				.then(image => {
					return image
						.resize(1000, Jimp.AUTO)     // resize
						.quality(80)                 // set JPEG quality
						.write(targetFilePath); // save
				})
				.catch(err => {
					console.error(err);
				}).then(image => {
					// Do stuff with the image.
					this.copyExifTool(sourceFilePath, targetFilePath, fileHash, function (fileHash) {
						resolve(fileHash);
					});
				});
				
		});

	}

	private copyExifTool(sourceFilePath, targetFilePath, fileHash, callback) {
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


	public async uploadTempFileChain(sourceFileHashesList : Array<string>): Promise<Array<string>> {

		var uploadTempFilePromises = [];
		sourceFileHashesList.forEach(hashItem => {
			uploadTempFilePromises.push(this.uploadTempFile(hashItem));
		});

		return new Promise<Array<string>>((resolve, reject) => {
			this.resolveSequentially(uploadTempFilePromises).then(( result : Array<any>) => { 
				resolve(result);
			})
		});
	}

	private async uploadTempFile(fileHash : string): Promise<string> {

		var uploadRequestOptions = this.requestOptions();
		uploadRequestOptions.uri = this.base_url + 'import/thumbnail/' + fileHash;
		uploadRequestOptions.encoding = 'binary';
		uploadRequestOptions.method = "POST";
		var fileHashLocation = path.join(this.getTempFolder(), fileHash + ".jpg");

		uploadRequestOptions.formData = {
				file: {
					value: fs.createReadStream(fileHashLocation),
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

		return new Promise<string>((resolve, reject) => {

			fs.access(fileHashLocation, fs.constants.F_OK, (err) => {
				if (err) {
					console.log(">>== skip: " + fileHash);
					resolve(fileHash);
				}
				else {
					console.log("----upload > ", fileHash);

					request(uploadRequestOptions)
						.then(function (uploadResults) {
							console.log("upload > ", uploadResults.body, fileHash);
							resolve(fileHash);
						})
						.catch(function (err) {
							console.log("uploadRequestOptions");
							console.log(err);
							resolve(fileHash);
						});
				}
			});

		});

	}

	public async deleteFileChain(sourceFileHashesList : Array<string>): Promise<Array<string>> {

		var deleteFilePromises = [];
		sourceFileHashesList.forEach(hashItem => {
			deleteFilePromises.push(this.deleteFile(hashItem));
		});

		return new Promise<Array<string>>((resolve, reject) => {
			this.resolveSequentially(deleteFilePromises).then(( result : Array<any>) => { 
				resolve(result);
			})
		});
	}

	private async deleteFile(fileHash : string): Promise<string> {

		return new Promise<string>((resolve, reject) => {
			var file1 = path.join(this.getTempFolder(), fileHash + ".jpg");
			fs.access(file1, fs.constants.F_OK, (err) => {
				if(err) return;
				fs.unlink(file1,function(err){
					if(err) return console.log(err);
				});
			});

			var file2 = path.join(this.getSourceTempFolder(), fileHash + ".jpg");
			fs.access(file2, fs.constants.F_OK, (err) => {
				if(err) return;
				fs.unlink(file2,function(err){
					if(err) return console.log(err);

					resolve();
				});
			});
		});

	}


}

// resizeImage(sourceFileHashesList[i],function (fileHash) {
// 	uploadTempFile(sourceFileHashesList, i, callback, finalCallback);
// })

// function getSearchStart(searchquery,pageNumber) {
// 	var indexRequestOptions = requestOptions;
// 	indexRequestOptions.uri = base_url + "search";
// 	indexRequestOptions.qs = {
// 		t: searchquery,
// 		json: 'true',
// 		p: pageNumber
// 	};
// 	getIndex(searchquery,indexRequestOptions);
// }




// function 

// function downloadSourceTempFile(sourceFileHashesList,i,searchquery) {


// 	var downloadrequestOptions = requestOptions;
// 	downloadrequestOptions.uri = base_url + 'api/thumbnail/' + sourceFileHashesList[i];
// 	downloadrequestOptions.method = "GET";
// 	downloadrequestOptions.encoding = 'UTF-8';
// 	delete downloadrequestOptions.formData;

// 	downloadrequestOptions.qs = {
// 		json: 'true',
// 		f: sourceFileHashesList[i]
// 	}


// 	request(downloadrequestOptions)
// 	    .then(function (result) {
// 			if(result.statusCode === 202) {
// 				console.log(result.statusCode, i, sourceFileHashesList.length, sourceFileHashesList[i]);
// 				downloadFromApiChain(sourceFileHashesList, i, searchquery, downloadSourceTempFile, done)
// 			}
// 			else {
// 				console.log(result.statusCode, i, sourceFileHashesList.length, sourceFileHashesList[i]);
// 				next(sourceFileHashesList, i, searchquery, downloadSourceTempFile, done)
// 			}
// 		})
// 	    .catch(function (err) {
//         	console.log(err.message, "i:", i, "len:", sourceFileHashesList.length, sourceFileHashesList[i]);
// 			console.log("downloadrequestOptions catch - " + sourceFileHashesList[i]);
// 			next(sourceFileHashesList, i, searchquery, downloadSourceTempFile, done)
// 	    });
// }

// function done() {

// 	if (maxPageNumber !== undefined && currentPageNumber <= maxPageNumber-1) {
// 		currentPageNumber++;
// 		getSearchStart(searchQuery,currentPageNumber);
// 	}
// 	else {
// 		console.log("-- everything is done :)");
// 	}

// }

// function downloadFromApiChain(sourceFileHashesList, i, searchquery, callback, finalCallback) {

// 	var downloadFilerequestOptions = requestOptions;
// 	downloadFilerequestOptions.uri = base_url + 'api/thumbnail/' + sourceFileHashesList[i];
// 	downloadFilerequestOptions.encoding = 'binary';
// 	downloadFilerequestOptions.method = "GET";
// 	downloadFilerequestOptions.qs = {
// 		f: sourceFileHashesList[i],
// 		issingleitem: 'true'
// 	}

// 	request(downloadFilerequestOptions)
// 		.then(function (fileResults) {
// 			var filePath = path.join(getSourceTempFolder(),sourceFileHashesList[i] + ".jpg");

// 			fs.writeFile(filePath, fileResults.body, 'binary', function (res) {
// 				resizeImage(sourceFileHashesList[i],function (fileHash) {
// 					uploadTempFile(sourceFileHashesList, i, callback, finalCallback);
// 				})
// 			});
// 		})
// 		.catch(function (err) {
// 			console.log("downloadFilerequestOptions");
// 			console.log(err);
// 			next(sourceFileHashesList, i, callback, finalCallback)
// 		});
// }

// function next(sourceFileHashesList, count, searchquery, callback, finalCallback) {
// 	deleteFile(sourceFileHashesList, count);

// 	count++;
// 	if(count < sourceFileHashesList.length) {
// 		callback(sourceFileHashesList, count, callback, finalCallback)
// 	}
// 	else {
// 		console.log("-- done query "+ searchquery +" (" + currentPageNumber + "/" + maxPageNumber +")");
// 		finalCallback(sourceFileHashesList);
// 	}
// }

// function uploadTempFile(sourceFileHashesList, i,callback, finalCallback) {
// 	var uploadRequestOptions = requestOptions;
// 	var fileHash = sourceFileHashesList[i];
// 	uploadRequestOptions.uri = base_url + 'import/thumbnail/' + fileHash;
// 	uploadRequestOptions.encoding = 'binary';
// 	uploadRequestOptions.method = "POST";

// 	var fileHashLocation = path.join(getTempFolder(), fileHash + ".jpg");


// 	uploadRequestOptions.formData = {
// 			file: {
// 				value: fs.createReadStream(fileHashLocation),
// 				options: {
// 					filename: fileHash + ".jpg",
// 					contentType: 'image/jpg'
// 				}
// 			}
// 		}

// 	uploadRequestOptions.qs = {
// 		f: fileHash,
// 		issingleitem: 'true'
// 	}

// 	fs.access(fileHashLocation, fs.constants.F_OK, (err) => {
// 		if (err) {
// 			console.log(">>== skip: " + fileHash);
// 			next(sourceFileHashesList, i, callback, finalCallback);
// 		}
// 		else {
// 			request(uploadRequestOptions)
// 				.then(function (uploadResults) {
// 					console.log("upload > ", uploadResults.body);
// 					next(sourceFileHashesList, i, callback, finalCallback);
// 				})
// 				.catch(function (err) {
// 					console.log("uploadRequestOptions");
// 					console.log(err);
// 				});
// 		}
// 	});



// }

// function deleteFile(sourceFileHashesList, i) {
// 	var file1 = path.join(getTempFolder(), sourceFileHashesList[i] + ".jpg");
// 	fs.access(file1, fs.constants.F_OK, (err) => {
// 		if(err) return;
// 		fs.unlink(file1,function(err){
// 			if(err) return console.log(err);
// 		});
// 	});

// 	var file2 = path.join(getSourceTempFolder(), sourceFileHashesList[i] + ".jpg");
// 	fs.access(file2, fs.constants.F_OK, (err) => {
// 		if(err) return;
// 		fs.unlink(file2,function(err){
// 			if(err) return console.log(err);
// 		});
// 	});
// }




// function resizeImage(fileHash,callback) {

// 	if(fileHash === undefined) {
// 		console.log("fileHash === undefined");
// 		return;
// 	}
// 	var sourceFilePath = path.join(getSourceTempFolder(),fileHash + ".jpg");
// 	var targetFilePath = path.join(getTempFolder(),fileHash + ".jpg");


// 	jimp.read(sourceFilePath).then(function (lenna) {
// 		return lenna.resize(1000, jimp.AUTO)     // resize
// 			.quality(80)                 // set JPEG quality
// 			.write(targetFilePath); // save
// 		}).then(image => {
// 			// Do stuff with the image.
// 			copyExiftool(sourceFilePath, targetFilePath, fileHash, function (fileHash) {
// 				callback(fileHash)
// 			});

// 	})
// 	.catch(function (err) {
// 		console.error(err);
// 		callback(fileHash);
// 	});

// }



// function copyExiftool(sourceFilePath, targetFilePath,fileHash, callback) {
// 	execFile(exiftool, ['-overwrite_original', '-TagsFromFile', sourceFilePath, targetFilePath, '-Orientation=', ], (error, stdout, stderr) => {
// 	    if (error) {
// 	        console.error(`exec error: ${error}`);
// 	        return;
// 	    }
// 	    // console.log(`stdout: ${stdout}`);
// 		if(stderr !== "") console.log(`stderr: ${stderr}`);
// 		return callback(fileHash);
// 	});
// }




// module.exports = {
// 	getSearchStart,
// 	downloadSourceTempFile,
// 	requestOptions
// }
