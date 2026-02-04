const https = require("https");


async function httpsPost(url, postData, authorizationHeader, contentType = "application/json") {
	return new Promise((resolve, reject) => {
		httpsPostInternal(url,postData, authorizationHeader, contentType).then(resolve).catch(()=> {
			httpsPostInternal(url,postData, authorizationHeader, contentType).then(resolve).catch((err)=> {
				console.log('failed');
				reject(err)
			})
		})
	});
}

/**
 * Handles the actual sending request.
 * We're turning the https.request into a promise here for convenience
 * @param url
 * @return {Promise}
 */
async function httpsPostInternal(url, postData, authorizationHeader, contentType = "application/json") {
	let requestOptionsUrl = new URL(url);
	const requestOptions = {
		host: requestOptionsUrl.host,
		path: requestOptionsUrl.pathname + requestOptionsUrl.search,
		method: "POST",
		headers: {
			"User-Agent": "Outlook-iOS/709.2226530.prod.iphone (3.24.1)",
			"Content-Type": contentType,
			'Content-Length': Buffer.byteLength(postData)
		},
	};
	if (authorizationHeader) {
		requestOptions.headers.Authorization = authorizationHeader;
	}

	// Promisify the https.request
	return new Promise((resolve, reject) => {
		// general request options, we defined that it's a POST request and content is JSON

		// actual request
		const req = https.request(requestOptions, (res) => {
			let response = "";

			res.on("data", (d) => {
				response += d;
			});

			// response finished, resolve the promise with data
			res.on("end", () => {
				try {
					const parsedResponse = JSON.parse(response)
					resolve(parsedResponse);
				} catch (error) {
					error.url = requestOptions.host + requestOptions.path
					error.statusCode = res.statusCode
					reject(error);
				}
			});
		});

		req.write(postData);

		// there was an error, reject the promise
		req.on("error", (e) => {
			reject(e);
		});

		req.end();
	});
}

module.exports = {
	httpsPost,
};
