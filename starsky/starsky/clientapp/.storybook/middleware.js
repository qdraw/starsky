const setRouter = require("../../../../starsky-tools/mock/set-router").setRouter;
const bodyParser = require("body-parser");

const expressMiddleWare = (router) => {
  if (!router) {
    return;
  }

  router.use(bodyParser.urlencoded({ extended: true }));

  router.get("/hello", (req, res) => {
    res.send("Hello World!");
    res.end();
  });

  setRouter(router, true);
};

module.exports = expressMiddleWare;
