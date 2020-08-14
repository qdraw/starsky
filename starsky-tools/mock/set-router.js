var apiAccountChangeSecretIndex = require('./api/account/change-secret/index.json')
var apiAccountPermissionsIndex = require('./api/account/permissions/index.json');

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
var apiUpdate__Starsky01dif20180101170001_Deleted = require('./api/update/__starsky_01-dif-2018.01.01.17.00.01_Deleted.json')
var apiUpdate__Starsky01dif20180101170001_Ok = require('./api/update/__starsky_01-dif-2018.01.01.17.00.01_Ok.json')

var apiEnvIndex = require('./api/env/index.json');
var apiPublishIndex = require('./api/publish/index.json');
var apiPublishCreateIndex = require('./api/publish/create/index.json');


function setRouter(app) {
  var prefix = "/starsky";

  app.get(prefix + '/', (req, res) => res.send('Hello World!'));

  app.get(prefix + '/account/status', (req, res) => res.json(accountStatus));


  var isChangePasswordSuccess = false;
  app.post(prefix + '/api/account/change-secret/', (req, res) => {

    console.log(req.body);

    if (!req.body.Password || req.body.Password.indexOf("error") >= 0) {
      res.statusCode = 401
      return res.json("Password is not correct")
    }

    isChangePasswordSuccess = !isChangePasswordSuccess;
    res.statusCode = !isChangePasswordSuccess ? 400 : 200;

    return res.json(!isChangePasswordSuccess ? "Model is not correct" : apiAccountChangeSecretIndex);
  });

  app.post(prefix + '/account/register', (req, res) => {
    return res.json("Account Created");
  });

  app.get(prefix + '/api/account/permissions', (req, res) => {
    res.json(apiAccountPermissionsIndex)
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
    res.statusCode = 404;
    return res.json("not found");
  });

  var isDeleted = true;
  app.post(prefix + '/api/update', (req, res) => {

    if (!req.body) {
      res.statusCode = 500;
      return res.json("no body, please include body");
    }

    if (req.body.f.startsWith("/__starsky/01-dif/")) {
      isDeleted = !isDeleted;
      res.statusCode = !isDeleted ? 404 : 200;
      return res.json(!isDeleted ? apiUpdate__Starsky01dif20180101170001_Deleted : apiUpdate__Starsky01dif20180101170001_Ok);
    }
    res.statusCode = 404;
    return res.json("not found");
  });

  app.post(prefix + '/api/replace', (req, res) => {

    if (!req.body) {
      res.statusCode = 500;
      return res.json("no body, please include body");
    }

    if (req.body.f.startsWith("/__starsky/01-dif/")) {
      isDeleted = !isDeleted;
      res.statusCode = !isDeleted ? 404 : 200;
      return res.json(!isDeleted ? apiUpdate__Starsky01dif20180101170001_Deleted : apiUpdate__Starsky01dif20180101170001_Ok);
    }
    res.statusCode = 404;
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

  app.get(prefix + '/api/env', (req, res) => {
    return res.json(apiEnvIndex)
  });

  app.post(prefix + '/api/env', (req, res) => {
    if (!req.body) {
      return res.json("no body ~ the normal api does ignore it");
    }

    const keys = Object.keys(req.body)
    keys.forEach(key => {
      apiEnvIndex[key] = req.body[key];
      if (req.body[key] === "true") {
        apiEnvIndex[key] = true;
      }
      if (req.body[key] === "false") {
        apiEnvIndex[key] = false;
      }
    });

    return res.json(apiEnvIndex)
  });

  app.get(prefix + '/api/health/application-insights', (req, res) => {
    res.set('Content-Type', 'application/javascript');
    return res.send("");
  });

  app.get(prefix + '/api/publish', (req, res) => {
    return res.json(apiPublishIndex)
  });

  app.post(prefix + '/api/publish/create', (req, res) => {
    return res.json(apiPublishCreateIndex)
  });

  app.get(prefix + '/api/publish/exist', (req, res) => {
    return res.json(req.query.itemName === "test");
  });

  // Simulate waiting
  var fakeLoading = {};
  app.get(prefix + '/export/zip/:id', (req, res) => {

    if (!fakeLoading[req.params.id]) {
      fakeLoading[req.params.id] = 0;
    }

    fakeLoading[req.params.id]++;

    if (fakeLoading[req.params.id] >= 4) {
      fakeLoading[req.params.id] = 0;
      return res.send("\"OK\"");
    }

    res.statusCode = 206;
    return res.send("\"Not ready\"");
  });

}

module.exports = {
  setRouter
};