const express = require("express");
const path = require("path");

const apiAccountChangeSecretIndex = require("./api/account/change-secret/index.json");
const apiAccountPermissionsIndex = require("./api/account/permissions/index.json");

const accountStatus = require("./api/account/status/index.json");
const apiHealthDetails = require("./api/health/details/index.json");
const apiHealthCheckForUpdates = require("./api/health/check-for-updates/index.json");
const apiGeoReverseLookup = require("./api/geo-reverse-lookup/index.json");

const apiIndexIndex = require("./api/index/index.json");
const apiIndex__Starsky = require("./api/index/__starsky.json");
const apiIndex0001 = require("./api/index/0001.json");
const apiIndex0001_toggleDeleted = require("./api/index/0001_toggleDeleted.json");

const apiIndex__Starsky01dif = require("./api/index/__starsky_01-dif.json");
const apiIndex__Starsky01difColorclass0 = require("./api/index/__starsky_01-dif_colorclass0.json");

const apiIndex__Starsky01dif20180101170001 = require("./api/index/__starsky_01-dif-2018.01.01.17.00.01.json");

const apiInfo__testJpg = require("./api/info/test.jpg.json");

const apiSearchTrash = require("./api/search/trash/index.json");
const apiSearch = require("./api/search/index.json");
const apiSearchTest = require("./api/search/test.json");
const apiSearchTest1 = require("./api/search/test1.json");
const apiUpdate__Starsky01dif20180101170001_Deleted = require("./api/update/__starsky_01-dif-2018.01.01.17.00.01_Deleted.json");
const apiUpdate__Starsky01dif20180101170001_Ok = require("./api/update/__starsky_01-dif-2018.01.01.17.00.01_Ok.json");

const apiEnvIndex = require("./api/env/index.json");
const apiEnvFeatures = require("./api/env/features.json");

const apiPublishIndex = require("./api/publish/index.json");
const apiPublishCreateIndex = require("./api/publish/create/index.json");

const apiDeskopEditorOpen = require("./api/desktop-editor/open.json");

const githubComReposQdrawStarskyReleaseIndex = require("./github.com/repos/qdraw/starsky/releases/index.json");

function setRouter(app, isStoryBook = false) {
	const prefix = "/starsky";

	app.use(
		prefix + "/api/thumbnail",
		express.static(path.join(__dirname, "api", "thumbnail"))
	);

	app.get(prefix + "/", (_, res) =>
		res.send("Mock of backend api!, also run clientapp")
	);

	if (!isStoryBook) {
		app.get("/", (_, res) =>
			res.send("Mock of backend api!, also run clientapp")
		);
	}

	app.get(prefix + "/api/account/status", (req, res) => {
		res.statusCode = 200;
		res.json(accountStatus);
	});

	let isChangePasswordSuccess = false;
	app.post(prefix + "/api/account/change-secret/", (req, res) => {
		console.log(req.body);

		if (!req.body.Password || req.body.Password.indexOf("error") >= 0) {
			res.statusCode = 401;
			return res.json("Password is not correct");
		}

		isChangePasswordSuccess = !isChangePasswordSuccess;
		res.statusCode = !isChangePasswordSuccess ? 400 : 200;

		return res.json(
			!isChangePasswordSuccess
				? "Model is not correct"
				: apiAccountChangeSecretIndex
		);
	});

	app.post(prefix + "/api/account/register", (req, res) => {
		return res.json("Account Created");
	});

	app.get(prefix + "/api/account/permissions", (req, res) => {
		res.json(apiAccountPermissionsIndex);
	});

	app.get(prefix + "/api/health/details", (req, res) => {
		res.status(503);
		res.json(apiHealthDetails);
	});

	app.get(prefix + "/api/health/check-for-updates", (req, res) => {
		res.status(202);
		res.json(apiHealthCheckForUpdates);
	});

	app.get(prefix + "/api/geo-reverse-lookup", (req, res) => {
		res.status(200);
		res.json(apiGeoReverseLookup);
	});

	app.get(prefix + "/api/info", (req, res) => {
		if (req.query.f === "/test.jpg") {
			return res.json(apiInfo__testJpg);
		}
		res.statusCode = 404;
		return res.json("not found");
	});

	app.get(prefix + "/api/index", (req, res) => {
		if (!req.query.f || req.query.f === "/") {
			if (req.query.collections === "false") {
				apiIndexIndex.collections = false;
			}
			return res.json(apiIndexIndex);
		}
		if (req.query.f === "/__starsky") {
			if (req.query.collections === "false") {
				apiIndex__Starsky.collections = false;
			}
			return res.json(apiIndex__Starsky);
		}
		if (req.query.f === "/0001") {
			if (req.query.collections === "false") {
				apiIndex0001.collections = false;
			}
			return res.json(apiIndex0001);
		}
		if (req.query.f === "/0001/toggleDeleted.jpg") {
			if (req.query.collections === "false") {
				apiIndex0001_toggleDeleted.collections = false;
			}
			return res.json(apiIndex0001_toggleDeleted);
		}
		if (
			req.query.f === "/__starsky/01-dif" &&
			req.query.colorClass === "0"
		) {
			if (req.query.collections === "false") {
				apiIndex__Starsky01difColorclass0.collections = false;
			}
			return res.json(apiIndex__Starsky01difColorclass0);
		}
		if (req.query.f === "/__starsky/01-dif") {
			if (req.query.collections === "false") {
				apiIndex__Starsky01dif.collections = false;
			}
			return res.json(apiIndex__Starsky01dif);
		}
		if (req.query.f.startsWith("/__starsky/01-dif/")) {
			if (req.query.collections === "false") {
				apiIndex__Starsky01dif20180101170001.collections = false;
			}
			return res.json(apiIndex__Starsky01dif20180101170001);
		}
		res.statusCode = 404;
		return res.json("not found");
	});

	let isDeleted = true;
	app.post(prefix + "/api/update", (req, res) => {
		if (!req.body) {
			res.statusCode = 500;
			return res.json("no body, please include body");
		}

		if (req.body.f.startsWith("/__starsky/01-dif/")) {
			isDeleted = !isDeleted;
			res.statusCode = !isDeleted ? 404 : 200;
			return res.json(
				!isDeleted
					? apiUpdate__Starsky01dif20180101170001_Deleted
					: apiUpdate__Starsky01dif20180101170001_Ok
			);
		}
		res.statusCode = 404;
		return res.json("not found");
	});

	app.post(prefix + "/api/replace", (req, res) => {
		if (!req.body) {
			res.statusCode = 500;
			return res.json("no body, please include body");
		}

		if (req.body.f.startsWith("/__starsky/01-dif/")) {
			isDeleted = !isDeleted;
			res.statusCode = !isDeleted ? 404 : 200;
			return res.json(
				!isDeleted
					? apiUpdate__Starsky01dif20180101170001_Deleted
					: apiUpdate__Starsky01dif20180101170001_Ok
			);
		}
		res.statusCode = 404;
		return res.json("not found");
	});

	app.get(prefix + "/api/search/trash", (req, res) => {
		return res.json(apiSearchTrash);
	});

	app.get(prefix + "/api/search", (req, res) => {
		if (req.query.t === "test" && req.query.p === "1") {
			return res.json(apiSearchTest1);
		}
		if (req.query.t === "test") {
			return res.json(apiSearchTest);
		}
		return res.json(apiSearch);
	});

	app.get(prefix + "/api/suggest", (req, res) => {
		res.setHeader("Cache-Control", "public, max-age=0");
		res.setHeader("Expires", 0);
		if (req.query.t === "test") {
			return res.json(["test", "testung"]);
		}
		return res.json([]);
	});

	app.get(prefix + "/api/env", (req, res) => {
		return res.json(apiEnvIndex);
	});

	app.get(prefix + "/api/env/features", (req, res) => {
		return res.json(apiEnvFeatures);
	});

	app.post(prefix + "/api/env", (req, res) => {
		if (!req.body) {
			return res.json("no body ~ the normal api does ignore it");
		}

		const keys = Object.keys(req.body);
		keys.forEach((key) => {
			apiEnvIndex[key] = req.body[key];
			if (req.body[key] === "true") {
				apiEnvIndex[key] = true;
			}
			if (req.body[key] === "false") {
				apiEnvIndex[key] = false;
			}
		});

		return res.json(apiEnvIndex);
	});

	app.post(prefix + "/api/desktop-editor/amount-confirmation", (req, res) => {
		if (!req.body) {
			return res.json("no body ~ the normal api does ignore it");
		}
		console.log(`amount-confirmation ${req.body.f}`);

		return res.json(req.body.f !== "/true.jpg");
	});

	app.post(prefix + "/api/desktop-editor/open", (req, res) => {
		if (!req.body) {
			return res.json("no body ~ the normal api does ignore it");
		}
		console.log(`open ${req.body.f}`);

		if (req.body.f === "/true.jpg") {
			res.statusCode = 400;
			res.json()
			return
		}

		return res.json(apiDeskopEditorOpen);
	});

	app.get(prefix + "/api/health/application-insights", (req, res) => {
		res.set("Content-Type", "application/javascript");
		return res.send("");
	});

	app.get(prefix + "/api/download-photo", (req, res) => {
		if (req.query.f == "/test.gpx") {
			const file = `${__dirname}/api/download-photo/test.gpx`;
			return res.download(file); // Set disposition and send it.
		}

		res.statusCode = 404;
		return res.send("not found");
	});

	app.get(prefix + "/api/publish", (req, res) => {
		return res.json(apiPublishIndex);
	});

	app.post(prefix + "/api/publish/create", (req, res) => {
		return res.json(apiPublishCreateIndex);
	});

	app.post(prefix + "/api/import", (req, res) => {
		setTimeout(() => {
			return res.json("");
		}, 3000);
	});

	app.get(prefix + "/api/publish/exist", (req, res) => {
		return res.json(req.query.itemName === "test");
	});

	// Simulate waiting
	let fakeLoading = {};
	app.get(prefix + "/export/zip/:id", (req, res) => {
		if (!fakeLoading[req.params.id]) {
			fakeLoading[req.params.id] = 0;
		}

		fakeLoading[req.params.id]++;

		if (fakeLoading[req.params.id] >= 4) {
			fakeLoading[req.params.id] = 0;
			return res.send('"OK"');
		}

		res.statusCode = 206;
		return res.send('"Not ready"');
	});

	/**
	 * Mock a github api
	 */
	app.get("/github.com/repos/qdraw/starsky/releases", (req, res) => {
		return res.send(githubComReposQdrawStarskyReleaseIndex);
	});
}

module.exports = {
	setRouter,
};
