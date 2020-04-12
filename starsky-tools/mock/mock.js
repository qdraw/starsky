const express = require('express')
const app = express()
const port = 5000
var accountStatus = require('./account/status/index.json')
var apiHealthDetails = require('./api/health/details/index.json')
var apiIndexIndex = require('./api/index/index.json')
var apiIndex__Starsky = require('./api/index/__starsky.json');
var apiIndex__Starsky01dif = require('./api/index/__starsky_01-dif.json')
var apiIndex__Starsky01difColorclass0 = require('./api/index/__starsky_01-dif_colorclass0.json')

var apiIndex__Starsky01dif20180101170001 = require('./api/index/__starsky_01-dif-2018.01.01.17.00.01.json')
var apiSearchTrash = require('./api/search/trash/index.json')
var apiSearch = require('./api/search/index.json')
var apiSearchTest = require('./api/search/test.json')
var apiSearchTest1 = require('./api/search/test1.json')

var prefix = "/starsky";

app.get(prefix + '/', (req, res) => res.send('Hello World!'));

app.get(prefix + '/account/status', (req, res) => res.json(accountStatus));


app.post(prefix + '/account/register', (req, res) => {
  return res.json("Account Created");
});


app.get(prefix + '/api/health/details', (req, res) => {
  res.status(503);
  res.json(apiHealthDetails)
});

app.get(prefix + '/api/index', (req, res) => {

  if (!req.query.f || req.query.f === "/") {
    return res.json(apiIndexIndex);
  }
  if (req.query.f === "/__starsky") {
    return res.json(apiIndex__Starsky);
  }
  if (req.query.f === "/__starsky/01-dif" && req.query.colorClass === "0") {
    return res.json(apiIndex__Starsky01difColorclass0);
  }
  if (req.query.f === "/__starsky/01-dif") {
    return res.json(apiIndex__Starsky01dif);
  }
  if (req.query.f.startsWith("/__starsky/01-dif/")) {
    return res.json(apiIndex__Starsky01dif20180101170001);
  }

  return res.json("not found");
});


app.get(prefix + '/api/search/trash', (req, res) => {
  return res.json(apiSearchTrash)
});

app.get(prefix + '/api/search', (req, res) => {
  if (req.query.t === "test" && req.query.p === "1") {
    return res.json(apiSearchTest1);
  }
  if (req.query.t === "test") {
    return res.json(apiSearchTest);
  }
  return res.json(apiSearch)
});

app.get(prefix + '/api/suggest', (req, res) => {
  if (req.query.t === "test") {
    return res.json(["test", "testung"]);
  }
  return res.json([])
});


app.listen(port, () => console.log(`Starsky mock app listening on port ${port}!`))
