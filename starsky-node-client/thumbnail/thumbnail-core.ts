
// var path = require('path');
import * as path from 'path';

// var fs = require('fs');
import * as fs from 'fs';

import jimp from 'jimp';
import sharp from 'sharp';

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

		var lastPageNumberList = Array<number>();
		await axiosResponses.forEach((response: AxiosResponse<IResults>)  => {

			
			if(response.data.searchCount >= 1  ) {
				response.data.fileIndexItems.forEach(fileIndexItem => {
					fileHashList.push(fileIndexItem.fileHash);
				});
			}

			// SUPPORT FOR PAGINATION
			lastPageNumberList.push(response.data.lastPageNumber);
		});

		// SUPPORT FOR PAGINATION
		for (let index = 0; index < lastPageNumberList.length; index++) {

			const lastPageNumber = lastPageNumberList[index];

			// console.log('lastPageNumber,', lastPageNumber);

			if(lastPageNumber >= 1) {

				var multiPageUrls = Array<AxiosRequestConfig>();
				for (let lpIndex = 1; lpIndex <= lastPageNumber; lpIndex++) {
					// console.log('index,', lpIndex,  filePathList[index],  );
					multiPageUrls.push(this.searchRequestOptions(filePathList[index],index)) 
				}
	
				const axiosMultiResponses = await Promise.all(
					multiPageUrls.map(queue.wrap(async (url : AxiosRequestConfig) 	=> 
					await axios.request(url)))
				);

				axiosMultiResponses.forEach((response: AxiosResponse<IResults>)  => {
					
					response.data.fileIndexItems.forEach(fileIndexItem => {
						fileHashList.push(fileIndexItem.fileHash);
					});
				});

		
			}
		}
		// END SUPPORT FOR PAGINATION

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
						
				writer.on('finish', resolve) // not able to return bool
				writer.on('error', resolve)
			
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


					// sharp(sourceFilePath)
					// 	.rotate()
					// 	.resize({ width: 1000 })
					// 	.toFile(targetFilePath)
					// 	.then(() => {
					
					// 		process.stdout.write("≈");
					// 		resolve(true);

					// }).catch(() => {
					// 	resolve(false);
					// });

					await jimp.read(sourceFilePath)
						.then(image => {
							image.resize(1000, jimp.AUTO);
							image.write(targetFilePath);
							
							process.stdout.write("≈");
							resolve(true);
						})
						.catch(err => {
							console.error(err);
							resolve(false);
						});	
										
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

	public async uploadTempFile(fileHash : string): Promise<boolean> {

		var uploadRequestOptions = this.requestOptions();
		uploadRequestOptions.url = this.base_url + 'import/thumbnail/' + fileHash;
		// uploadRequestOptions.encoding = 'binary';
		uploadRequestOptions.method = "POST";

		var fileHashLocation = path.join(this.getTempFolder(), fileHash + ".jpg");
		uploadRequestOptions.data = fs.createReadStream(fileHashLocation);


		uploadRequestOptions.headers['Content-Type'] = 'image/jpeg';
		uploadRequestOptions.headers['filename'] = fileHash + ".jpg";

		return new Promise<boolean>((resolve, reject) => {

			fs.access(fileHashLocation, fs.constants.F_OK, (err) => {
				if (err) {
					console.log(">>== skip: " + fileHash);
					resolve(false);
				}

				fs.stat(fileHashLocation,(err, stats) => {
					if( err || stats.size <= 50 ) return resolve(false);


					Axios(uploadRequestOptions).then((response : AxiosResponse) => {

						//console.log("upload > ", response.status, response.data, fileHash);
						process.stdout.write("∑");
						resolve(false);
					}).catch(function (thrown) {
						console.log(thrown);
						resolve(false);
					});
				});

				
			});

		});

	}

	public async deleteSourceTempFolder(): Promise<void> {
		return this.removeContentOfDirectory(this.getSourceTempFolder());
	}

	public async deleteTempFolder(): Promise<void> {
		return this.removeContentOfDirectory(this.getTempFolder());
	}

	public async removeContentOfDirectory(dirPath : string): Promise<void> {
		var content = await fs.promises.readdir(dirPath);

		await content.forEach(element => {
			var location = path.join(dirPath,element);
			fs.promises.unlink(location);
		});
	}

}
