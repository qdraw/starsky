var url = require("url");
const axios = require("axios");

module.exports = function getAccessTokenFromRefreshToken(
	clientId,
	clientSecret,
	refreshToken
) {
	return new Promise((resolve, reject) => {
		const params = new url.URLSearchParams({
			refresh_token: refreshToken,
			grant_type: "refresh_token",
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
				if (response.data && response.data.access_token) {
					resolve(response.data.access_token);
					return;
				}
				resolve(false);
			})
			.catch(function (thrown) {
				console.log(thrown);
				resolve(false);
			});
	});
};
