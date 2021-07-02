import {
	AxiosError,
	AxiosRequestConfig,
	AxiosResponse,
	default as axios,
	default as Axios,
} from "axios";
import { TaskQueue } from "cwait";
import * as fs from "fs";
import jimp from "jimp";
import * as path from "path";
import { IResults } from "./IResults";

var execFile = require("child_process").execFile;
var exiftool = require("dist-exiftool");

export interface ISizes {
	ok: boolean;
	small: boolean;
	large: boolean;
	extraLarge: boolean;
	fileHash: string;
}

export class Parser {
	public parseRanges(args: string[]): string[] {
		var start = Number(args[0].split("-")[0]);
		var end = Number(args[0].split("-")[1]);

		if (isNaN(start) || isNaN(end)) {
			console.log("Use numbers in a range (now searching for " + args[0] + ")");
			return [args[0]];
		}

		if (start >= end) {
			console.log(">> Rejected << Use the lowest value first");
			return [];
		}

		var queries = [];
		for (let index = start; index <= end; index++) {
			if (index === 0) {
				// Search for today (use 0)
				queries.push("-Datetime>0 -ImageFormat:jpg -!delete");
				continue;
			}
			queries.push(
				"-Datetime>" +
					index +
					" -Datetime<" +
					(index - 1) +
					" -ImageFormat:jpg -!delete"
			);
		}
		return queries;
	}
}

export class Files {
	public RemoveOldFiles() {
		this.cleanFolder(path.join(__dirname, "temp"));
		this.cleanFolder(path.join(__dirname, "source_temp"));
	}

	private cleanFolder(folderPath: string) {
		fs.readdir(folderPath, function (_err, files) {
			if (!files) return; // ignore if not exist
			files.forEach(function (file) {
				fs.stat(path.join(folderPath, file), function (err, stat) {
					var endTime, now;
					if (err) {
						return console.error(err);
					}
					now = new Date().getTime();
					endTime = new Date(stat.ctime).getTime() + 3600000 * 5; // 5 hours
					if (now > endTime && stat.isFile) {
						fs.unlink(path.join(folderPath, file), () => {});
					}
				});
			});
		});
	}
}

export class Query {
	base_url: string;
	access_token: string;

	public readonly MAX_SIMULTANEOUS_DOWNLOADS = 10;

	constructor(base_url: string, access_token: string) {
		this.base_url = base_url;
		this.access_token = access_token;

		// Make sure the output directories exist
		this.getRights();
	}

	public requestOptions(): AxiosRequestConfig {
		return {
			url: this.base_url,
			method: "GET",
			headers: {
				"User-Agent": "MS FrontPage Express",
				Authorization: "Basic " + this.access_token,
			},
		};
	}

	public async isImportOrDirectSearch(searchQuery: string): Promise<any> {
		if (searchQuery === "IMPORT") {
			var filePathList: Array<string> = await this.isImportIndex();
			return await this.searchIndexList(filePathList, true);
		} else {
			return this.searchIndexList([searchQuery], false);
		}
	}

	private getSourceTempFolder() {
		return path.join(__dirname, "source_temp");
	}

	private getTempFolder() {
		return path.join(__dirname, "temp");
	}

	public async searchIndexList(
		filePathList: Array<string>,
		isFilePath: boolean = false
	): Promise<Array<string>> {
		const urls = Array<AxiosRequestConfig>();

		filePathList.forEach((element) => {
			if (isFilePath) {
				urls.push(this.searchRequestOptions(`-filePath:"${element}"`));
			} else {
				urls.push(this.searchRequestOptions(element));
			}
		});

		const queue = new TaskQueue(Promise, this.MAX_SIMULTANEOUS_DOWNLOADS);
		const axiosResponses = await Promise.all(
			urls.map(
				queue.wrap(async (url: AxiosRequestConfig) => await axios.request(url))
			)
		);

		var fileHashList = Array<string>();

		var lastPageNumberList = Array<number>();
		axiosResponses.forEach((response: AxiosResponse<IResults>) => {
			// To Display the query as is
			if (!isFilePath) {
				if (response.data.searchFor === undefined) {
					console.log(
						"<<< FATAL ERROR: >>> \n the field searchFor is missing in the API"
					);
					return;
				}
				process.stdout.write("¶øπ¶ ");
				response.data.searchFor.forEach((search) => {
					process.stdout.write(search + ", ");
				});
				process.stdout.write(" ¶πø¶\n");
			}
			if (response.data.searchCount >= 1) {
				response.data.fileIndexItems.forEach((fileIndexItem) => {
					if (fileIndexItem.imageFormat === "jpg") {
						fileHashList.push(fileIndexItem.fileHash);
					}
				});
			}
			// SUPPORT FOR PAGINATION
			lastPageNumberList.push(response.data.lastPageNumber);
		});

		// SUPPORT FOR PAGINATION
		for (let index = 0; index < lastPageNumberList.length; index++) {
			const lastPageNumber = lastPageNumberList[index];

			if (lastPageNumber >= 1) {
				var multiPageUrls = Array<AxiosRequestConfig>();
				for (let lpIndex = 1; lpIndex <= lastPageNumber; lpIndex++) {
					multiPageUrls.push(
						this.searchRequestOptions(filePathList[index], lpIndex)
					);
				}

				const axiosMultiResponses = await Promise.all(
					multiPageUrls.map(
						queue.wrap(
							async (url: AxiosRequestConfig) => await axios.request(url)
						)
					)
				);

				axiosMultiResponses.forEach(
					(multiPageResponse: AxiosResponse<IResults>) => {
						multiPageResponse.data.fileIndexItems.forEach((fileIndexItem) => {
							if (fileIndexItem.imageFormat === "jpg") {
								fileHashList.push(fileIndexItem.fileHash);
							}
						});
					}
				);
			}
		}
		// END SUPPORT FOR PAGINATION

		return fileHashList;
	}

	public async checkIfSingleFileNeedsToBeDownloaded(
		hashItem: string
	): Promise<ISizes> {
		var downloadFileRequestOptions = this.requestOptions();
		downloadFileRequestOptions.url =
			this.base_url + "api/thumbnail/list-sizes/" + hashItem;
		downloadFileRequestOptions.method = "GET";

		downloadFileRequestOptions.params = {
			json: "true",
		};

		const defaultFail = {
			fileHash: hashItem,
			ok: false,
			small: false,
			large: false,
			extraLarge: false,
		} as ISizes;

		return await axios(downloadFileRequestOptions)
			.then(function (response: AxiosResponse) {
				if (
					response.status !== 210 &&
					response.status !== 202 &&
					response.status !== 200 &&
					response.status !== 404
				) {
					console.log(response);
				}

				if (response.status === 202) {
					process.stdout.write("•");
					return {
						ok: true,
						small: response.data.small,
						large: response.data.large,
						extraLarge: response.data.extraLarge,
						fileHash: hashItem,
					} as ISizes;
				}
				process.stdout.write("≠");

				return defaultFail;
			})
			.catch(function (err: AxiosError) {
				console.log(
					"checkIfSingleFileNeedsToBeDownloaded ==> ",
					err.response.status,
					err.config.url
				);
				return defaultFail;
			});
	}

	public downloadBinarySingleFile(hashItem: string): Promise<boolean> {
		this.getRights();

		var downloadFileRequestOptions = this.requestOptions();
		downloadFileRequestOptions.url =
			this.base_url + "api/thumbnail/" + hashItem;
		downloadFileRequestOptions.responseType = "stream";
		downloadFileRequestOptions.method = "GET";
		downloadFileRequestOptions.params = {
			f: hashItem,
			issingleitem: "true",
		};

		var filePath = path.join(this.getSourceTempFolder(), hashItem + ".jpg");

		return new Promise((resolve, reject) => {
			Axios(downloadFileRequestOptions)
				.then((response: AxiosResponse) => {
					const writer = fs.createWriteStream(filePath);

					response.data.pipe(writer);

					writer.on("finish", resolve); // not able to return bool
					writer.on("error", resolve);
				})
				.catch(function (thrown) {
					resolve(false);
				});
		});
	}

	public async resizer(
		size: number,
		sourceFilePath: string,
		targetPath: string
	): Promise<boolean> {
		return new Promise<boolean>((resolve, reject) => {
			if (!fs.existsSync(sourceFilePath)) {
				console.log(`file does not exist ${sourceFilePath} size: ${size}`);
				resolve(false);
			}

			jimp
				.read(sourceFilePath)
				.then((image) => {
					image.resize(size, jimp.AUTO);
					image.quality(80);

					image.write(targetPath, () => {
						process.stdout.write("≈");
						resolve(true);
					});
				})
				.catch((err) => {
					console.error(err);
					resolve(false);
				});
		});
	}

	public async resizeImage(fileHash: string): Promise<boolean> {
		var sourceFilePath = path.join(
			this.getSourceTempFolder(),
			fileHash + ".jpg"
		);
		var targetExtraLargeFilePath = path.join(
			this.getTempFolder(),
			fileHash + "@2000.jpg"
		);
		var targetLargeFilePath = path.join(
			this.getTempFolder(),
			fileHash + ".jpg"
		);
		var targetSmallFilePath = path.join(
			this.getTempFolder(),
			fileHash + "@300.jpg"
		);

		return new Promise<boolean>((resolve, reject) => {
			fs.access(sourceFilePath, fs.constants.F_OK, async (err) => {
				// Very important!!
				if (err !== null) {
					process.stdout.write("†");
					resolve(false);
				}

				if (err === null) {
					// // Sharp example code
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

					if (
						!(await this.resizer(
							2000,
							sourceFilePath,
							targetExtraLargeFilePath
						))
					) {
						resolve(false);
					}
					if (
						!(await this.resizer(
							1000,
							targetExtraLargeFilePath,
							targetLargeFilePath
						))
					) {
						resolve(false);
					}
					if (
						!(await this.resizer(300, targetLargeFilePath, targetSmallFilePath))
					) {
						resolve(false);
					}
					resolve(true);
				}
			});
		});
	}

	private copyExifTool(sourceFilePath, targetFilePath, fileHash, callback) {
		fs.stat(targetFilePath, (err, stats) => {
			if (err || stats.size <= 50) return callback(fileHash);

			// '-overwrite_original',
			execFile(
				exiftool,
				["-TagsFromFile", sourceFilePath, targetFilePath, "-Orientation="],
				(error, stdout, stderr) => {
					if (error) {
						console.error(`exec error: ${error}`);
						return;
					}
					// console.log(`stdout: ${stdout}`);
					if (stderr !== "") console.log(`stderr: ${stderr}`);
					process.stdout.write("~");
					return callback(fileHash);
				}
			);
		});
	}

	public searchRequestOptions(
		searchQuery: string,
		pageNumber = 0
	): AxiosRequestConfig {
		var indexRequestOptions: AxiosRequestConfig = this.requestOptions();
		indexRequestOptions.url = this.base_url + "api/search";
		indexRequestOptions.params = {
			t: searchQuery,
			json: "true",
			p: pageNumber,
		};
		return indexRequestOptions;
	}

	public async isImportIndex(): Promise<Array<string>> {
		var importRequestOptions = this.requestOptions();
		importRequestOptions.url = this.base_url + "api/import/history/";

		// TODO REFACTOR@@@@
		const ops = [];
		let op = axios(importRequestOptions);
		ops.push(op);

		let allQueryResult: Array<AxiosResponse> = await axios.all(ops);
		var returnBaseQueryList = new Array<string>();

		allQueryResult.forEach((oneQuery) => {
			console.log(oneQuery.data.length);

			for (let index = 0; index < oneQuery.data.length; index++) {
				const item = oneQuery.data[index];
				if (item === undefined || item === null || item.fileHash.length !== 26)
					continue;
				returnBaseQueryList.push(item.filePath);
			}
		});

		return returnBaseQueryList;
	}

	private ensureExistsFolder(path, mask, cb) {
		if (typeof mask == "function") {
			// allow the `mask` parameter to be optional
			cb = mask;
			mask = parseInt("0777", 8);
		}
		fs.mkdir(path, mask, function (err) {
			if (err) {
				if (err.code == "EEXIST") cb(null);
				// ignore the error if the folder already exists
				else cb(err); // something else went wrong
			} else cb(null); // successfully created folder
		});
	}

	private getRights() {
		this.ensureExistsFolder(
			this.getSourceTempFolder(),
			parseInt("0744", 8),
			function (err) {
				if (err) console.log(err); // handle folder creation error
			}
		);
		this.ensureExistsFolder(
			this.getTempFolder(),
			parseInt("0744", 8),
			function (err) {
				if (err) console.log(err); // handle folder creation error
			}
		);
	}

	public async uploadTempFile(fileHash: string): Promise<boolean> {
		return new Promise<boolean>((resolve, reject) => {
			this.uploadAxios(fileHash + "@300.jpg", fileHash, (status1) => {
				this.uploadAxios(fileHash + ".jpg", fileHash, (status2) => {
					this.uploadAxios(fileHash + "@2000.jpg", fileHash, (status3) => {
						resolve(status1 && status2 && status3);
					});
				});
			});
		});
	}

	private uploadAxios(fileName: string, fileHash: string, next: Function) {
		const uploadRequestOptions = this.requestOptions();
		uploadRequestOptions.url = this.base_url + "api/import/thumbnail/";
		uploadRequestOptions.method = "POST";

		uploadRequestOptions.headers["Content-Type"] = "image/jpeg";

		const fileHashLocation = path.join(this.getTempFolder(), fileName);
		uploadRequestOptions.data = fs.createReadStream(fileHashLocation);

		uploadRequestOptions.headers["filename"] = fileName;

		fs.access(fileHashLocation, fs.constants.F_OK, (err) => {
			if (err) {
				console.log(">>== skip: " + fileHash);
				next(false);
			}

			fs.stat(fileHashLocation, (err, stats) => {
				if (err || stats.size <= 50) {
					console.log(
						">>== skip * err:: " + err + "~  stats size:",
						stats.size
					);
					return next(false);
				}

				Axios(uploadRequestOptions)
					.then((response: AxiosResponse) => {
						process.stdout.write("∑");
						next(true);
					})
					.catch(function (thrown: AxiosError) {
						let statusCode = 0;
						if (thrown && thrown.response && thrown.response.status) {
							statusCode = thrown.response.status;
						}
						const errorMessage =
							"upload failed: " + thrown.config.url + " " + statusCode;
						console.log(errorMessage);
						if (statusCode === 405 || statusCode === 502) {
							console.log("¢");
							setTimeout(() => {
								next(false);
							}, 5000);
							return;
						}
						next(false);
					});
			});
		});
	}

	public deleteSourceTempFolder(fileHashList: string[]) {
		this.removeContentOfDirectory(this.getSourceTempFolder(), fileHashList);
	}

	public deleteTempFolder(fileHashList: string[]) {
		this.removeContentOfDirectory(this.getTempFolder(), fileHashList);
	}

	public deleteSourceTempFile(fileHash: string) {
		// 1 size stored here
		const location = path.join(this.getSourceTempFolder(), fileHash + ".jpg");
		fs.unlink(location, () => {});
	}

	public deleteTempFile(fileHash: string) {
		const location1000px = path.join(this.getTempFolder(), fileHash + ".jpg");
		const location300px = path.join(
			this.getTempFolder(),
			fileHash + "@300.jpg"
		);
		const location2000px = path.join(
			this.getTempFolder(),
			fileHash + "@2000.jpg"
		);

		if (fs.existsSync(location1000px)) {
			fs.unlink(location1000px, () => {});
		}
		if (fs.existsSync(location300px)) {
			fs.unlink(location300px, () => {});
		}
		if (fs.existsSync(location2000px)) {
			fs.unlink(location2000px, () => {});
		}
	}

	private removeContentOfDirectory(dirPath: string, fileHashList: string[]) {
		fileHashList.forEach((element) => {
			var location = path.join(dirPath, element);
			fs.unlink(location, () => {});
		});
	}
}
