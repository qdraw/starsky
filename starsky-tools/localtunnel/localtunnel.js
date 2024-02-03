var express = require("express");
var expressWs = require("express-ws");
var httpProxy = require("http-proxy");
var WebSocket = require("ws");
var cookieParser = require("cookie-parser");
var dotenv = require("dotenv");
const localtunnel = require("@security-patched/localtunnel");

dotenv.config();

const helpModal =
	process.argv.indexOf("--help") >= 0 || process.argv.indexOf("-h") >= 0;

if (helpModal) {
	console.log("Usage: node localtunnel.js");
	console.log("Disable sockets: node localtunnel.js --no-websocket");
	process.exit(0);
}

// https://github.com/http-party/node-http-proxy/issues/891#issuecomment-419412499

var proxyTargetSettings = {
	secure: false,
	changeOrigin: true,
	secure: false,
	autoRewrite: true,
	xfwd: true,
};
const proxy = httpProxy.createProxyServer(proxyTargetSettings);

const httpServer = express();
const wsServer = expressWs(httpServer);

// cookie parser middleware
httpServer.use(cookieParser());

// // custom middleware
// httpServer.use((req, res, next) => {
//   // put your custom middleware here
//   next()
// });

var createReactAppRouteUrl = "http://0.0.0.0:3000/";

if (process.env.STARSKYURL) {
	console.log("Warning: missing SUBDOMAIN env variable but this is optional");
}

// To change for example to a different domain
if (process.env.STARSKYURL) {
	netCoreAppRouteUrl = process.env.STARSKYURL;
	netCoreAppRouteUrl += netCoreAppRouteUrl.endsWith("/") ? "" : "/";
	console.log("running on " + netCoreAppRouteUrl);
} else {
	console.log("missing .env file with STARSKYURL (back-end)");
	process.exit(1);
}

const noWebSocket =
	process.argv.indexOf("--no-websocket") >= 0 ||
	process.argv.indexOf("-nw") >= 0 ||
	process.argv.indexOf("--no-socket") >= 0;

if (noWebSocket) {
	console.log("websocket disabled");
}

// register websocket handler, proxy requests manually to backend
if (!noWebSocket) {
	console.log("websocket enabled");

	wsServer.app.ws("/starsky/realtime", (ws, req) => {
		console.log("--");
		console.log(req.headers["sec-websocket-key"]);
		let headers = {};
		// add custom headers, e.g. copy cookie if required
		if (req.headers["cookie"]) {
			headers["cookie"] = req.headers["cookie"];
		}
		if (req.headers.authorization) {
			headers.authorization = req.headers.authorization;
		}

		const backendMessageQueue = [];
		let backendConnected = false;
		let backendClosed = false;
		let frontendClosed = false;

		const socketUrl =
			netCoreAppRouteUrl.replace("https://", "wss://") +
			"starsky/realtime";
		console.log(socketUrl);

		const backendSocket = new WebSocket(socketUrl, [], {
			headers: headers,
		});
		backendSocket.on("open", function () {
			console.log("backend connection established");
			backendConnected = true;
			// send queued messages
			backendMessageQueue.forEach((message) => {
				backendSocket.send(message);
			});
		});
		backendSocket.on("error", (err) => {
			console.log("error", err);
			backendClosed = true;
			if (!frontendClosed) {
				ws.close();
			}
			frontendClosed = true;
		});
		backendSocket.on("message", (message) => {
			// proxy messages from backend to frontend
			try {
				ws.send(message);
			} catch (error) {}
		});
		backendSocket.on("close", (statusCode) => {
			console.log("Backend is closing, reason: " + statusCode);
			backendClosed = true;
			if (!frontendClosed) {
				try {
					ws.close(statusCode);
				} catch (error) {
					console.log(error);
				}
			}
			frontendClosed = true;
		});
		ws.on("message", (message) => {
			// proxy messages from frontend to backend
			if (backendConnected) {
				backendSocket.send(message);
			} else {
				backendMessageQueue.push(message);
			}
		});
		ws.on("close", () => {
			console.log("Frontend is closing");
			frontendClosed = true;
			if (!backendClosed) {
				backendSocket.close();
			}
			backendClosed = true;
		});
	});
}

// proxy http requests to proxy target
httpServer.all("/**", (req, res) => {
	if (req.url === "/starsky/realtime") {
		res.writeHead(400, { "content-type": "application/json" });
		res.end('"use websockets"');
		return;
	}
	const toProxyUrl = createReactAppRouteUrl;
	if (
		req.originalUrl.startsWith("/api") ||
		// before login visit http://localhost:4000/account/login
		req.originalUrl.startsWith("/account/login") ||
		req.originalUrl.startsWith("/starsky/api") ||
		req.originalUrl.startsWith("/starsky/account") ||
		req.originalUrl.startsWith("/starsky/sync/") ||
		req.originalUrl.startsWith("/starsky/export/") ||
		req.originalUrl.startsWith("/starsky/realtime")
	) {
		toProxyUrl = netCoreAppRouteUrl;
	}

	// Watch for Secure Cookies and remove the secure-label
	proxy.on("proxyRes", function (proxyRes, req, res, options) {
		const sc = proxyRes.headers["set-cookie"];
		if (Array.isArray(sc)) {
			proxyRes.headers["set-cookie"] = sc.map((sc) => {
				return sc
					.split(";")
					.filter((v) => v.trim().toLowerCase() !== "secure")
					.join("; ");
			});
		}
	});

	proxy.web(
		req,
		res,
		{
			...proxyTargetSettings,
			cookieDomainRewrite: {
				"*": req.headers.host,
			},
			target: toProxyUrl,
		},
		(error) => {
			console.log("Could not contact proxy backend", error);
			try {
				res.writeHead(502, { "content-type": "application/json" });
				res.end('"The service is not available right now."');
			} catch (e) {
				console.log("Could not send error message to client", e);
			}
		}
	);
});

// setup express
const port = process.env.PORT || process.env.port || 6501;
httpServer.listen(port);
console.log("http://localhost:" + port);

const http = require("http");
const https = require("https");

// To check if the services are ready/started
[netCoreAppRouteUrl, createReactAppRouteUrl].forEach((url) => {
	if (url.startsWith("https")) {
		https
			.get(url, (resp) => {})
			.on("error", (err) => {
				console.log("Error: " + err.message);
			});
	} else {
		http.get(url, (resp) => {}).on("error", (err) => {
			console.log("Error: " + err.message);
		});
	}
});

function localTunnelR() {
	// LocalTunnel Setup

	(async () => {
		// lt -p 8080 -h http://localtunnel.me --local-https false
		const tunnel = await localtunnel({
			subdomain: process.env.SUBDOMAIN,
			host: "http://localtunnel.me",
			port: port,
			local_https: false,
		}).catch((err) => {
			throw err;
		});

		console.log(tunnel);
		
		// the assigned public url for your tunnel
		// i.e. https://abcdefgjhij.localtunnel.me
		console.log("Your localtunnel is ready on:");
		console.log(tunnel.url);

		tunnel.on("error", () => {
			console.log("err");
		});

		tunnel.on("close", () => {
			console.log("tunnels are closed");
		});
	})();
}

localTunnelR();
