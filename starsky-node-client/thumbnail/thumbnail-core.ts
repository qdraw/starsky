
var path = require('path');
var fs = require('fs');

// var Jimp = require('jimp'); //es6 -> fails
import jimp from 'jimp';
import sharp from 'sharp';

// import { OptionsWithUri, FullResponse } from "request-promise-native";

// import request = require('request-promise-native');

import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';

import { IResults } from "./IResults";
import Axios from 'axios';
import { TaskQueue } from 'cwait';
var execFile = require('child_process').execFile;
var exiftool = require('dist-exiftool');


export class Query {
	base_url: string;
	access_token: string;

	public readonly  MAX_SIMULTANEOUS_DOWNLOADS  =  10;

	constructor(base_url: string, access_token : string) {		
		this.base_url = base_url;
		this.access_token = access_token;
	}

	public requestOptions() : AxiosRequestConfig {
		return {
			url: this.base_url,
			method: "GET",
			headers: {
				'User-Agent': 'MS FrontPage Express',
				'Authorization': 'Basic ' + this.access_token,
			},
		}
	};

	public async isImportOrDirectSearch(searchQuery : string) :  Promise<any> {
		if(searchQuery === "IMPORT") {
	
			var filePathList : Array<string> = await this.isImportIndex();
			return await this.searchIndexList(filePathList,true);
		}
		else {
			return this.searchIndexList([searchQuery],false);
		}
	}
	
	private getSourceTempFolder() {
		return path.join(__dirname, "source_temp");
	}

	private getTempFolder() {
		return path.join(__dirname, "temp");
	}

	public async searchIndexList(filePathList : Array<string>, isFilePath: boolean = false) :  Promise<Array<string>> {

		const urls = Array<AxiosRequestConfig>();

		filePathList.forEach(element => {
			if(isFilePath) {
				urls.push(this.searchRequestOptions(`-filePath:"${element}"`))
			}
			else {
				urls.push(this.searchRequestOptions(element))
			}
		});
	
		const queue = new TaskQueue(Promise, this.MAX_SIMULTANEOUS_DOWNLOADS);
		const axiosResponses = await Promise.all(urls.map(queue.wrap(async (url : AxiosRequestConfig) 	=> 
				await axios.request(url))));

		var fileHashList = Array<string>();
		axiosResponses.forEach(response => {

			// TODO: MISSING SUPPORT FOR PAGINATION
			
			if(response.data.searchCount >= 1) {
				response.data.fileIndexItems.forEach(fileIndexItem => {
					fileHashList.push(fileIndexItem.fileHash);
				});
			}
		});

		return fileHashList;

	}


	public async checkIfSingleFileNeedsToBeDownloaded(hashItem : string): Promise<boolean> {

		var downloadFileRequestOptions = this.requestOptions();;
		downloadFileRequestOptions.url = this.base_url + 'api/thumbnail/' + hashItem;
		downloadFileRequestOptions.method = "GET";

		downloadFileRequestOptions.params = {
			json: 'true',
			f: hashItem
		}

		return await axios(downloadFileRequestOptions)
			.then(function (response) {

				if(response.status === 202) {
					process.stdout.write(".");
					return true;
				}
				return false;
		}).catch(function (err) {
			console.log('checkIfSingleFileNeedsToBeDownloaded ==> ',err.response)
			return false;
		});

	}

	public downloadBinarySingleFile(hashItem : string): Promise<boolean> {

		this.getRights();

		var downloadFileRequestOptions = this.requestOptions();
		downloadFileRequestOptions.url = this.base_url + 'api/thumbnail/' + hashItem;
		downloadFileRequestOptions.responseType = 'stream'
		downloadFileRequestOptions.method = "GET";
		downloadFileRequestOptions.params = {
			f: hashItem,
			issingleitem: 'true'
		}

		var filePath = path.join(this.getSourceTempFolder(), hashItem + ".jpg");

		return new Promise((resolve, reject) => {
			Axios(downloadFileRequestOptions).then((response : AxiosResponse) => {
				const writer = fs.createWriteStream(filePath)
			
				response.data.pipe(writer);
						
				writer.on('finish', resolve(true))
				writer.on('error', resolve(false))
			
			}).catch(function (thrown) {
				resolve(false);
			});
		})
		
	}

	public async resizeImage(fileHash : string): Promise<boolean> {

		var sourceFilePath = path.join(this.getSourceTempFolder(),fileHash + ".jpg");
		var targetFilePath = path.join(this.getTempFolder(),fileHash + ".jpg");


		return new Promise<boolean>((resolve, reject) => {

			fs.access(sourceFilePath, fs.constants.F_OK, async (err) => {

				// Very important!!
				if (err) resolve(false);

				if (!err) {


					sharp(sourceFilePath)
						.rotate()
						.resize({ width: 1000 })
						.toFile(targetFilePath)
						.then(() => {
					
							process.stdout.write("≈");
							resolve(true);

					}).catch(() => {
						resolve(false);
					});

					// await jimp.read(sourceFilePath)
					// 	.then(image => {
					// 		image.resize(1000, jimp.AUTO);
					// 		image.write(targetFilePath);
							
					// 		process.stdout.write("≈");
					// 		resolve(true);
					// 	})
					// 	.catch(err => {
					// 		console.error(err);
					// 		resolve(false);
					// 	});	
										
				}

			});	
		});

	}

	private copyExifTool(sourceFilePath, targetFilePath, fileHash, callback) {
		
		fs.stat(targetFilePath,(err, stats) => {
			if( err || stats.size <= 50 ) return callback(fileHash);

			// '-overwrite_original',
			execFile(exiftool, [ '-TagsFromFile', sourceFilePath, targetFilePath, '-Orientation=', ], (error, stdout, stderr) => {
				if (error) {
					console.error(`exec error: ${error}`);
					return;
				}
				// console.log(`stdout: ${stdout}`);
				if(stderr !== "") console.log(`stderr: ${stderr}`);
				process.stdout.write("~");
				return callback(fileHash);
			});

		});
	}

	public searchRequestOptions(searchQuery: string, pageNumber = 0) : AxiosRequestConfig {
	
		var indexRequestOptions : AxiosRequestConfig = this.requestOptions();
		indexRequestOptions.url = this.base_url + "search";
		indexRequestOptions.params = {
			t: searchQuery,
			json: 'true',
			p: pageNumber
		};
		return indexRequestOptions;
	}

	public async isImportIndex() : Promise<Array<string>>  {

		var importRequestOptions = this.requestOptions();
		importRequestOptions.url = this.base_url + 'import/history/';
	
		// TODO REFACTOR@@@@
		const ops = [];
		let op = axios(importRequestOptions);
		ops.push(op);

		let allQueryResult : Array<AxiosResponse> = await axios.all(ops);
		var returnBaseQueryList = new Array<string>();

		allQueryResult.forEach(oneQuery => {

			console.log(oneQuery.data.length);

			for (let index = 0; index < oneQuery.data.length; index++) {
				const item = oneQuery.data[index];
				if(item === undefined ||  item === null ||  item.fileHash.length !== 26) continue;
				returnBaseQueryList.push(item.filePath);
			}

		});

		return returnBaseQueryList;

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



	private getRights() {
		this.ensureExistsFolder(this.getSourceTempFolder(), parseInt('0744',8) , function(err) {
			if (err) console.log(err);// handle folder creation error
		});
		this.ensureExistsFolder(this.getTempFolder(), parseInt('0744',8), function(err) {
			if (err) console.log(err);// handle folder creation error
		});
	}


}

	
// export class Query {

// 	base_url: string;
// 	access_token: string;


// 	constructor(base_url: string, access_token : string) {		
// 		this.base_url = base_url;
// 		this.access_token = access_token;
// 	}

// 	private resolveSequentially(arr) {
// 		let r = Promise.resolve();
// 		return Promise.all(
// 		  arr.reduce((p, c) => {
// 			r = r.then(() => (typeof c === 'function' ? c() : c));
// 			p.push(r);
// 			return p;
// 		  }, []),
// 		);
// 	}

// 	public requestOptions() : OptionsWithUri {
// 		return {
// 			uri: this.base_url,
// 			method: "GET",
// 			headers: {
// 				'User-Agent': 'MS FrontPage Express',
// 				'Authorization': 'Basic ' + this.access_token,
// 			},
// 			resolveWithFullResponse: true,
// 			json: true // Automatically parses the JSON string in the response
// 		}
// 	};

// 	public indexRequestOptions(searchQuery: string, pageNumber = 0) : OptionsWithUri {
	
// 		var indexRequestOptions : OptionsWithUri = this.requestOptions();
// 		indexRequestOptions.uri = this.base_url + "search";
// 		indexRequestOptions.qs = {
// 			t: searchQuery,
// 			json: 'true',
// 			p: pageNumber
// 		};
// 		return indexRequestOptions;
// 	}




// 	public parseFileIndexItems(items : FullResponse) : Array<string>{

// 		var fileHashList = new Array<string>();
// 		for (var i in items.body.fileIndexItems) {

// 			var item = items.body.fileIndexItems[i];
// 			if(item === undefined ||  item === null ||  item.fileHash.length !== 26) continue;
// 			if(item.imageFormat !== "jpg") continue;
// 			fileHashList.push(item.fileHash)
// 		}
// 		return fileHashList;
// 	}

// 	public async searchIndex(indexRequestOptions : OptionsWithUri): IResults {
// 		return await request(indexRequestOptions)
// 			.then(function (items : FullResponse) {
// 				return <IResults>{
// 					fileHashList : new Array<string>()
// 				};
// 		});
// 	}


// 	// public async searchIndexOLD(indexRequestOptions : OptionsWithUri): Promise<IResults> {

// 	// 	this.getRights();

// 	// 	var those = this;

// 	// 	const response = await axios.get(url);


// 	// 	return new Promise<IResults>((resolve, reject) => {

// 	// 		request(indexRequestOptions)
// 	// 			.then(function (items : FullResponse) {

// 	// 				if(items.body === undefined) reject();
// 	// 				if(items.body.fileIndexItems === undefined) reject();
		
// 	// 				var searchQuery = items.body.searchQuery

// 	// 				var fileHashList = those.parseFileIndexItems(items);
					
		
// 	// 				if(fileHashList.length === 0) console.log("> 0 imageFormat:jpg items");
		
// 	// 				var lastPageNumber = items.body.lastPageNumber;

// 	// 				if(fileHashList.length <= 0) {
// 	// 					reject(<IResults>{
// 	// 						fileHashList : new Array<string>()
// 	// 					});
// 	// 				}
// 	// 				else {
// 	// 					var result = <IResults>{
// 	// 						fileHashList,
// 	// 						lastPageNumber,
// 	// 						searchQuery
// 	// 					};

// 	// 					var requests = [];
// 	// 					for (var i = 1; i <= lastPageNumber; i++) {
// 	// 						var indexRequestOptions = those.indexRequestOptions(searchQuery,i);
// 	// 						requests.push(request(indexRequestOptions));
// 	// 					}
// 	// 					console.log(requests);

// 	// 					// All at one
// 	// 					Promise.all(requests).then(function (items) {

// 	// 						items.forEach(item => {
								
// 	// 							if(item.body.fileIndexItems === undefined) reject();
// 	// 							var fileHashList = those.parseFileIndexItems(item);

// 	// 							fileHashList.forEach(element => {
// 	// 								result.fileHashList.push(element);
// 	// 							});
// 	// 						});

// 	// 						resolve(result);

// 	// 					})
// 	// 					.catch(function (err) {
// 	// 						reject();
// 	// 					});
						
// 	// 				}
// 	// 			})
// 	// 			.catch(function (err) {
// 	// 				console.log(err);
// 	// 				reject(<IResults>{});
// 	// 				// API call failed...
// 	// 			});

// 	// 	});


		
// 	// }

// 	public async checkIfSingleFileNeedsToBeDownloadedApiChain(sourceFileHashesList : Array<string>): Promise<Array<string>> {


// 		var downloadPromises = [];
// 		sourceFileHashesList.forEach(hashItem => {
// 			downloadPromises.push(this.checkIfSingleFileNeedsToBeDownloaded(hashItem));
// 		});

// 		return new Promise<Array<string>>((resolve, reject) => {

// 			if(sourceFileHashesList.length === 0) reject([]);

// 			this.resolveSequentially(downloadPromises).then(( result : Array<any>) => { 

// 				var filtered : Array<string> = result.filter(function (el) {
// 					return el != null;
// 				});

// 				resolve(filtered);
// 			})
// 		});
			
// 	}

// 	public async checkIfSingleFileNeedsToBeDownloaded(hashItem : string): Promise<string> {

// 		var downloadFileRequestOptions = this.requestOptions();;
// 		downloadFileRequestOptions.uri = this.base_url + 'api/thumbnail/' + hashItem;
// 		downloadFileRequestOptions.method = "GET";
// 		downloadFileRequestOptions.encoding = 'UTF-8';
// 		delete downloadFileRequestOptions.formData;

// 		downloadFileRequestOptions.qs = {
// 			json: 'true',
// 			f: hashItem
// 		}

// 		return new Promise<string>((resolve, reject) => {

// 			request(downloadFileRequestOptions)
// 				.then(function (result : FullResponse) {
					
// 					if(result.statusCode === 202) {
// 						console.log(hashItem, result.statusCode);
// 						resolve(hashItem)
// 					}
// 					else {
// 						resolve(null);
// 					}
// 				})
// 				.catch(function (err) {
// 					console.log(err.statusCode, "checkIfSingleFileNeedsToBeDownloaded");
// 					resolve(null);
// 				});
// 		});
// 	}



// 	public async isImportIndex(): Promise<Array<string>> {

// 		return new Promise<Array<string>>((resolve, reject) => {

// 			var importRequestOptions = this.requestOptions();
// 			importRequestOptions.uri = this.base_url + 'import/history/';

// 			request(importRequestOptions)
// 				.then(function (items) {

// 					if(items.body.length >= 1) {
// 						var filePathList = new Array<string>();

// 						for (var i in items.body) {
// 							var item = items.body[i];
// 							if(item === undefined ||  item === null ||  item.fileHash.length !== 26) continue;
// 							filePathList.push(item.filePath);
// 						}
// 						resolve(filePathList);

// 					}
// 				})
// 				.catch(function (err) {
// 					console.log(err);
// 					// API call failed...
// 					reject();
// 				});
	
// 		});
			
// 	}

// 	public async downloadBinarySingleFile(hashItem : string): Promise<string> {

// 		var those = this; 
// 		var downloadFileRequestOptions = this.requestOptions();
// 		downloadFileRequestOptions.uri = this.base_url + 'api/thumbnail/' + hashItem;
// 		downloadFileRequestOptions.encoding = 'binary';
// 		downloadFileRequestOptions.method = "GET";
// 		downloadFileRequestOptions.qs = {
// 			f: hashItem,
// 			issingleitem: 'true'
// 		}
// 		return new Promise<string>((resolve, reject) => {

// 			request(downloadFileRequestOptions)
// 				.then(function (fileResults) {
// 					var filePath = path.join(those.getSourceTempFolder(), hashItem + ".jpg");
// 					fs.writeFile(filePath, fileResults.body, 'binary', function (res) {
// 						resolve(hashItem);
// 					});
// 				})
// 				.catch(function (err) {
// 					console.log("downloadBinarySingleFile");
// 					console.log(err.statusCode);

// 					reject();

// 				});
// 		});

// 	}


// 	public async downloadBinaryApiChain(sourceFileHashesList : Array<string>): Promise<Array<string>> {

// 		var downloadPromises = [];
// 		sourceFileHashesList.forEach(hashItem => {
// 			if(hashItem != null ) {
// 				downloadPromises.push(this.downloadBinarySingleFile(hashItem));
// 			};
// 		});

// 		console.log('downloadBinaryApiChain sourceFileHashesList', sourceFileHashesList)

// 		return new Promise<Array<string>>((resolve, reject) => {
// 			this.resolveSequentially(downloadPromises).then(( result : Array<any>) => { 
// 				resolve(result);
// 			})
// 		});
// 	}



// 	public async resizeChain(sourceFileHashesList : Array<string>): Promise<Array<string>> {

// 		var resizePromises = [];
// 		sourceFileHashesList.forEach(hashItem => {
// 			resizePromises.push(this.resizeImage(hashItem));
// 		});

// 		return new Promise<Array<string>>((resolve, reject) => {
// 			this.resolveSequentially(resizePromises).then(( result : Array<any>) => { 
// 				resolve(result);
// 			})
// 		});

// 	}

// 	public async resizeImage(fileHash : string): Promise<string> {

// 		var sourceFilePath = path.join(this.getSourceTempFolder(),fileHash + ".jpg");
// 		var targetFilePath = path.join(this.getTempFolder(),fileHash + ".jpg");

// 		return new Promise<string>((resolve, reject) => {

// 			if(fileHash === undefined) {
// 				console.log("fileHash === undefined");
// 				reject();
// 			}

// 			Jimp.read(sourceFilePath)
// 				.then(image => {
// 					return image
// 						.resize(1000, Jimp.AUTO)     // resize
// 						.quality(80)                 // set JPEG quality
// 						.write(targetFilePath); // save
// 				})
// 				.catch(err => {
// 					console.error(err);
// 				}).then(image => {
// 					// Do stuff with the image.
// 					this.copyExifTool(sourceFilePath, targetFilePath, fileHash, function (fileHash) {
// 						resolve(fileHash);
// 					});
// 				});
				
// 		});

// 	}

// 	private copyExifTool(sourceFilePath, targetFilePath, fileHash, callback) {
// 		execFile(exiftool, ['-overwrite_original', '-TagsFromFile', sourceFilePath, targetFilePath, '-Orientation=', ], (error, stdout, stderr) => {
// 			if (error) {
// 				console.error(`exec error: ${error}`);
// 				return;
// 			}
// 			// console.log(`stdout: ${stdout}`);
// 			if(stderr !== "") console.log(`stderr: ${stderr}`);
// 			return callback(fileHash);
// 		});
// 	}


// 	public async uploadTempFileChain(sourceFileHashesList : Array<string>): Promise<Array<string>> {

// 		var uploadTempFilePromises = [];
// 		sourceFileHashesList.forEach(hashItem => {
// 			uploadTempFilePromises.push(this.uploadTempFile(hashItem));
// 		});

// 		return new Promise<Array<string>>((resolve, reject) => {
// 			this.resolveSequentially(uploadTempFilePromises).then(( result : Array<any>) => { 
// 				resolve(result);
// 			})
// 		});
// 	}

// 	private async uploadTempFile(fileHash : string): Promise<string> {

// 		var uploadRequestOptions = this.requestOptions();
// 		uploadRequestOptions.uri = this.base_url + 'import/thumbnail/' + fileHash;
// 		uploadRequestOptions.encoding = 'binary';
// 		uploadRequestOptions.method = "POST";
// 		var fileHashLocation = path.join(this.getTempFolder(), fileHash + ".jpg");

// 		uploadRequestOptions.formData = {
// 				file: {
// 					value: fs.createReadStream(fileHashLocation),
// 					options: {
// 						filename: fileHash + ".jpg",
// 						contentType: 'image/jpg'
// 					}
// 				}
// 			}

// 		uploadRequestOptions.qs = {
// 			f: fileHash,
// 			issingleitem: 'true'
// 		}

// 		return new Promise<string>((resolve, reject) => {

// 			fs.access(fileHashLocation, fs.constants.F_OK, (err) => {
// 				if (err) {
// 					console.log(">>== skip: " + fileHash);
// 					resolve(fileHash);
// 				}
// 				else {
// 					request(uploadRequestOptions)
// 						.then(function (uploadResults) {
// 							console.log("upload > ", uploadResults.body, fileHash);
// 							resolve(fileHash);
// 						})
// 						.catch(function (err) {
// 							console.log("uploadRequestOptions");
// 							console.log(err);
// 							resolve(fileHash);
// 						});
// 				}
// 			});

// 		});

// 	}

// 	public async deleteFileChain(sourceFileHashesList : Array<string>): Promise<Array<string>> {

// 		var deleteFilePromises = [];
// 		sourceFileHashesList.forEach(hashItem => {
// 			deleteFilePromises.push(this.deleteFile(hashItem));
// 		});

// 		return new Promise<Array<string>>((resolve, reject) => {
// 			this.resolveSequentially(deleteFilePromises).then(( result : Array<any>) => { 
// 				resolve(result);
// 			})
// 		});
// 	}

// 	private async deleteFile(fileHash : string): Promise<string> {

// 		return new Promise<string>((resolve, reject) => {
// 			var file1 = path.join(this.getTempFolder(), fileHash + ".jpg");
// 			fs.access(file1, fs.constants.F_OK, (err) => {
// 				if(err) return;
// 				fs.unlink(file1,function(err){
// 					if(err) return console.log(err);
// 				});
// 			});

// 			var file2 = path.join(this.getSourceTempFolder(), fileHash + ".jpg");
// 			fs.access(file2, fs.constants.F_OK, (err) => {
// 				if(err) return;
// 				fs.unlink(file2,function(err){
// 					if(err) return console.log(err);

// 					resolve();
// 				});
// 			});
// 		});

// 	}


// }