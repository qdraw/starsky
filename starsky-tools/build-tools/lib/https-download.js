const fs = require("fs");
const https = require("https");

async function httpsDownload(url, filePath, authorizationHeader = "") {
	return new Promise((resolve, reject) => {
		httpsDownloadInternal(url, filePath, authorizationHeader)
			.then(resolve)
			.catch(() => {
				httpsDownloadInternal(url, filePath, authorizationHeader)
					.then(resolve)
					.catch((err) => {
						console.log("failed");
						reject(err);
					});
			});
	});
}

async function httpsDownloadInternal(url, filePath, authorizationHeader = "") {
	let requestOptionsUrl = new URL(url);
	const requestOptions = {
		host: requestOptionsUrl.host,
		path: requestOptionsUrl.pathname + requestOptionsUrl.search,
		method: "GET",
		headers: {
			"User-Agent": "Outlook-iOS/709.2226530.prod.iphone (3.24.1)",
			Accept: "*/*",
			"Accept-Encoding": "gzip, deflate, br",
			Connection: "keep-alive",
		},
	};
	if (authorizationHeader) {
		requestOptions.headers.Authorization = authorizationHeader;
	}

	return new Promise((resolve, reject) => {
		const file = fs.createWriteStream(filePath);

		const request = https.get(requestOptions, (response) => {
			// check if response is success
			if (response.statusCode !== 200) {
				const error = {
					url: "https://" + requestOptions.host + requestOptions.path,
					statusCode: response.statusCode,
					headers: response.headers,
				};
				reject(error);
			}

			response.pipe(file);
		});

		// close() is async, call cb after close completes
		file.on("finish", () => file.close(resolve));

		// check for request error too
		request.on("error", (err) => {
			fs.unlink(filePath, () => {
				const error = {
					message: err.message,
					url: requestOptions.host + requestOptions.path,
				};
				reject(error);
			}); // delete the (partial) file and then return the error
		});

		file.on("error", (err) => {
			// Handle errors
			fs.unlink(filePath, () => {
				const error = {
					message: err.message,
					url: requestOptions.host + requestOptions.path,
				};
				reject(error);
			}); // delete the (partial) file and then return the error
		});
	});
}

module.exports = {
	httpsDownload,
};
