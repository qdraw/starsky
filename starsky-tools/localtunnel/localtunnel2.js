var express = require('express');
var expressWs = require('express-ws');
var httpProxy = require('http-proxy');
var WebSocket = require('ws');
var cookieParser = require('cookie-parser');
var dotenv = require('dotenv');
dotenv.config();

// https://github.com/http-party/node-http-proxy/issues/891#issuecomment-419412499

var proxyTargetSettings = {
  secure: false,
  changeOrigin: true,
  secure: false,
  autoRewrite: true,
  xfwd: true,
}
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

var createReactAppRouteUrl = 'http://localhost:3000/';

// To change for example to a different domain
if (process.env.STARSKYURL) {
  netCoreAppRouteUrl = process.env.STARSKYURL;
  console.log('running on ' + netCoreAppRouteUrl);
}


// register websocket handler, proxy requests manually to backend
wsServer.app.ws("/starsky/realtime", (ws, req) => {
  console.log('--');
  let headers = {};
  // add custom headers, e.g. copy cookie if required
  if (req.headers["cookie"]) {
    headers["cookie"] = req.headers["cookie"];
  }
  if (req.headers.authorization) {
    headers.authorization = req.headers.authorization
  }

  const backendMessageQueue = [];
  let backendConnected = false;
  let backendClosed = false;
  let frontendClosed = false;

  var socketUrl = netCoreAppRouteUrl.replace("https://", "wss://") + "starsky/realtime";
  console.log(socketUrl);

  const backendSocket = new WebSocket(socketUrl, [], {
    headers: headers
  });
  backendSocket.on('open', function () {
    console.log('backend connection established');
    backendConnected = true;
    // send queued messages
    backendMessageQueue.forEach(message => {
      backendSocket.send(message);
    });
  });
  backendSocket.on('error', (err) => {
    console.log('error', err);
    backendClosed = true;
    if (!frontendClosed) {
      ws.close();
    }
    frontendClosed = true;
  });
  backendSocket.on('message', (message) => {
    // proxy messages from backend to frontend
    ws.send(message);
  });
  backendSocket.on('close', () => {
    console.log('Backend is closing');
    backendClosed = true;
    if (!frontendClosed) {
      ws.close();
    }
    frontendClosed = true;
  });
  ws.on('message', (message) => {
    // proxy messages from frontend to backend
    if (backendConnected) {
      backendSocket.send(message);
    } else {
      backendMessageQueue.push(message);
    }
  });
  ws.on('close', () => {
    console.log('Frontend is closing');
    frontendClosed = true;
    if (!backendClosed) {
      backendSocket.close();
    }
    backendClosed = true;
  });
});


// proxy http requests to proxy target
httpServer.all("/**", (req, res) => {
  if (req.url === "/starsky/realtime") {
    res.writeHead(400, { 'content-type': 'application/json' });
    res.end("\"use websockets\"");
    return;
  }
  var toProxyUrl = createReactAppRouteUrl;
  if (req.originalUrl.startsWith("/starsky/api") ||
    req.originalUrl.startsWith("/starsky/account") ||
    req.originalUrl.startsWith("/starsky/sync/") ||
    req.originalUrl.startsWith("/starsky/export/") ||
    req.originalUrl.startsWith("/starsky/realtime")) {
    toProxyUrl = netCoreAppRouteUrl
  }

  // Watch for Secure Cookies and remove the secure-label
  proxy.on('proxyRes', function (proxyRes, req, res, options) {
    const sc = proxyRes.headers['set-cookie'];
    if (Array.isArray(sc)) {
      proxyRes.headers['set-cookie'] = sc.map(sc => {
        return sc.split(';')
          .filter(v => v.trim().toLowerCase() !== 'secure')
          .join('; ')
      });
    }
  });

  proxy.web(req, res, {
    ...proxyTargetSettings,
    cookieDomainRewrite: {
      '*': req.headers.host
    },
    target: toProxyUrl
  },
    (error) => {
      console.log('Could not contact proxy backend', error);
      try {
        res.send("The service is not available right now.");
      } catch (e) {
        console.log('Could not send error message to client', e);
      }
    });
});

// setup express
var port = process.env.PORT || process.env.port || 6501;
httpServer.listen(port);
console.log("http://localhost:" + port);

const http = require('http');
const https = require('https');

// To check if the services are ready/started
[netCoreAppRouteUrl, createReactAppRouteUrl].forEach(url => {
  if (url.startsWith("https")) {
    https.get(url, (resp) => {
    }).on("error", (err) => {
      console.log("Error: " + err.message);
    });
  }
  else {
    http.get(url, (resp) => {
    }).on("error", (err) => {
      console.log("Error: " + err.message);
    });
  }
});
