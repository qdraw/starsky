const https = require("https");

/**
 * Handles the actual sending request.
 * We're turning the https.request into a promise here for convenience
 * @param url
 * @return {Promise}
 */
async function httpsGet(url) {
	let requestOptionsUrl = new URL(url);
	const requestOptions = {
		host: requestOptionsUrl.host,
		path: requestOptionsUrl.pathname + requestOptionsUrl.search,
		method: "GET",
		headers: {
			"User-Agent": "Outlook-iOS/709.2226530.prod.iphone (3.24.1)",
			"Content-Type": "application/json",
		},
	};

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
				resolve(JSON.parse(response));
			});
		});

		// there was an error, reject the promise
		req.on("error", (e) => {
			reject(e);
		});

		req.end();
	});
}

module.exports = {
	httpsGet,
};
