import bodyParser from "body-parser";
import { setRouter } from "../../../../starsky-tools/mock/set-router.js";

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

export default expressMiddleWare;
