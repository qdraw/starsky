
const setRouter = require('../../../../starsky-tools/mock/set-router').setRouter;
var bodyParser = require('body-parser');

const expressMiddleWare = router => {
  router.use(bodyParser.urlencoded({ extended: true }));

  router.get('/hello', (req, res) => {
    res.send('Hello World!');
    res.end();
  });

  setRouter(router);
}

module.exports = expressMiddleWare