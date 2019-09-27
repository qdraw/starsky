var express = require('express');
var app = express();
var httpProxy = require('http-proxy');
var apiProxy = httpProxy.createProxyServer();
var dotenv = require('dotenv');
dotenv.config();

const http = require('http');
const https = require('https');

// all urls to proxy
app.all("/*", function (req, res, next) {
  if (req.originalUrl.startsWith("/api") || req.originalUrl.startsWith("/account") || req.originalUrl.startsWith("/suggest/")) {
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
      xfwd: true
    }
  );
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
      xfwd: true,
      cookieDomainRewrite: {
        '*': req.headers.host
      },
      ws: false,
    }
  );

  // Error: socket hang up
  apiProxy.on('error', function (error, req, res) {
    var json;
    if (!res.headersSent) {
      res.writeHead(500, { 'content-type': 'application/json' });
    }
    json = { error: 'proxy_error', reason: error.message };
    res.end(JSON.stringify(json));
  });
}

// setup express
var port = process.env.PORT || process.env.port || 6501;
app.listen(port);
console.log("http://localhost:" + port);

const localtunnel = require('localtunnel');

(async () => {
  const tunnel = await localtunnel({
    subdomain: process.env.SUBDOMAIN,
    port: port
  });

  // the assigned public url for your tunnel
  // i.e. https://abcdefgjhij.localtunnel.me
  console.log("Your localtunnel is ready on:");
  console.log(tunnel.url);

  tunnel.on('close', () => {
    // tunnels are closed
  });
})();
