const readline = require("readline");
var path = require("path");
require("dotenv").config({ path: path.join(__dirname, ".env") });
var url = require("url");
const axios = require("axios");

function askQuestion(query) {
	const rl = readline.createInterface({
		input: process.stdin,
		output: process.stdout,
	});

	return new Promise((resolve) =>
		rl.question(query, (ans) => {
			rl.close();
			resolve(ans);
		})
	);
}

function getToken(clientId, clientSecret, accessCode) {
	return new Promise((resolve, reject) => {
		const params = new url.URLSearchParams({
			code: accessCode,
			grant_type: "authorization_code",
		});

		var listQueryRequestOptions = {
			url: "https://api.dropbox.com/oauth2/token",
			method: "POST",
			data: params,
			auth: {
				username: clientId,
				password: clientSecret,
			},
		};

		axios(listQueryRequestOptions)
			.then((response) => {
				if (response.data && response.data.refresh_token) {
					resolve(response.data.refresh_token);
					return;
				}
				resolve(false);
			})
			.catch(function (thrown) {
				console.log(thrown);
				resolve(false);
			});
	});
}

(async () => {
	let clientId = process.env.DROPBOX_CLIENT_ID;
	if (!process.env.DROPBOX_CLIENT_ID) {
		clientId = await askQuestion(
			"Whats your dropbox clientID (dev portal)?\n"
		);
	}

	console.log(
		"Open the following url in your webbrowser and copy the access code:"
	);
	const url = `https://www.dropbox.com/oauth2/authorize?client_id=${clientId}&token_access_type=offline&response_type=code&scope=files.metadata.write files.permanent_delete files.metadata.read files.content.write files.content.read`;
	console.log(url);
	console.log("----");

	let clientSecret = process.env.DROPBOX_CLIENT_SECRET;
	if (!process.env.DROPBOX_CLIENT_SECRET) {
		clientSecret = await askQuestion(
			"Whats your dropbox client Secret (dev portal)?\n"
		);
	}

	const accessCode = await askQuestion(
		"Whats your dropbox access code? (copy from the site you opened)\n"
	);
	const token = await getToken(clientId, clientSecret, accessCode);
	if (token) {
		console.log("Save this token:");
		console.log("DROPBOX_REFRESH_TOKEN=" + token);
		console.log("DROPBOX_CLIENT_ID=" + clientId);
		console.log("DROPBOX_CLIENT_SECRET=" + clientSecret);
		console.log("to the .env file in the dropbox-import folder");
	}
})();
