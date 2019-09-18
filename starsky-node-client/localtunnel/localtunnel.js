var express = require('express');
var app = express();
var httpProxy = require('http-proxy');
var apiProxy = httpProxy.createProxyServer();
var dotenv = require('dotenv');
dotenv.config();

app.all("/*", function (req, res, next) {

  if (req.originalUrl.startsWith("/api") || req.originalUrl.startsWith("/account") ) {
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


var netCoreAppRouteUrl = 'http://localhost:5000/';
function NetCoreAppRouteRoute(req, res, next) {
    apiProxy.web(req, res,
        {
            target: netCoreAppRouteUrl,
            changeOrigin: true,
            secure: false,
            autoRewrite: true,
            xfwd: true
        }
    );
}


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
  console.log(tunnel.url);

  tunnel.on('close', () => {
    // tunnels are closed
  });
})();
