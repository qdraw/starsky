var express = require('express');
var app = express();
var httpProxy = require('http-proxy');
var apiProxy = httpProxy.createProxyServer();
var dotenv = require('dotenv');
dotenv.config();

const http = require('http');
const https = require('https');


// setup express
var port = process.env.PORT || process.env.port || 6501;
var server = require('http').createServer(app);
server.listen(port);
console.log("http://localhost:" + port);

// all urls to proxy
app.all("/*", function (req, res, next) {

  if (req.originalUrl.startsWith("/sockjs-node")) {
    res.status(503);
    res.json("not allowed");
  }

  if (req.originalUrl.startsWith("/starsky/api") ||
    req.originalUrl.startsWith("/starsky/account") ||
    req.originalUrl.startsWith("/starsky/sync/") ||
    req.originalUrl.startsWith("/starsky/export/") ||
    req.originalUrl.startsWith("/starsky/realtime")) {
    NetCoreAppRouteRoute(req, res, next);
  }
  else {
    CreateReactAppRoute(req, res, next);
  }
});

// To proxy the dev server
var createReactAppRouteUrl = 'http://localhost:3000/';
function CreateReactAppRoute(req, res, next) {
  apiProxy.web(req, res,
    {
      target: createReactAppRouteUrl,
      changeOrigin: true,
      secure: false,
      autoRewrite: true,
      xfwd: false
    }
  );
  // res.setHeader("Content-Security-Policy", "default-src 'self'; img-src 'self' https://*.tile.openstreetmap.org; script-src 'self' https://az416426.vo.msecnd.net 'nonce-53b3ebf63787426db506e8e49d4400c4'; connect-src 'self' https://dc.services.visualstudio.com; style-src 'unsafe-inline'; font-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'; object-src 'none'");
}

// To proxy the backend server
var netCoreAppRouteUrl = 'http://localhost:5000/';
if (!process.env.STARSKYURL) {
  console.log('running on default: use STARSKYURL to customize ');
}

// To change for example to a different domain
if (process.env.STARSKYURL) {
  netCoreAppRouteUrl = process.env.STARSKYURL;
  console.log('running on ' + netCoreAppRouteUrl);
}

var proxy = httpProxy.createProxy({
  ws: true,
  secure: false
});
server.on('upgrade', function (req, res) {
  proxy.ws(req, res, {
    target: netCoreAppRouteUrl
  }, function (e) {
    console.log(e, req);
  });
});

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


function NetCoreAppRouteRoute(req, res, next) {
  // Watch for Secure Cookies and remove the secure-label
  apiProxy.on('proxyRes', function (proxyRes, req, res, options) {
    const sc = proxyRes.headers['set-cookie'];
    if (Array.isArray(sc)) {
      proxyRes.headers['set-cookie'] = sc.map(sc => {
        return sc.split(';')
          .filter(v => v.trim().toLowerCase() !== 'secure')
          .join('; ')
      });
    }
  });

  apiProxy.web(req, res,
    {
      target: netCoreAppRouteUrl,
      changeOrigin: true,
      secure: false,
      autoRewrite: true,
      xfwd: false,
      cookieDomainRewrite: {
        '*': req.headers.host
      },
      ws: true,
    }
  );

  // Error: socket hang up
  apiProxy.on('error', function (error, req, res) {
    if (error && error.code && error.code === 'ECONNRESET') {
      return;
    }
    var json;
    if (!res.headersSent) {
      res.writeHead(500, { 'content-type': 'application/json' });
    }
    json = { error: 'proxy_error', reason: error.message };
    res.end(JSON.stringify(json));
  });
}

// LocalTunnel Setup
const localtunnel = require('localtunnel');

(async () => {

  // lt -p 8080 -h http://localtunnel.me --local-https false
  const tunnel = await localtunnel({
    subdomain: process.env.SUBDOMAIN,
    host: 'http://localtunnel.me',
    port: port,
    local_https: false
  }).catch(err => {
    throw err;
  });

  // the assigned public url for your tunnel
  // i.e. https://abcdefgjhij.localtunnel.me
  console.log("Your localtunnel is ready on:");
  console.log(tunnel.url);

  tunnel.on('error', () => {
    console.log('err');
  });

  tunnel.on('close', () => {
    console.log('tunnels are closed');
  });
})();
