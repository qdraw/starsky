const express = require('express')
const app = express()
const port = 5000;

var bodyParser = require('body-parser');
app.use(bodyParser.urlencoded({ extended: true }));

var setRouter = require('./set-router.js').setRouter;

setRouter(app);

app.listen(port, () => console.log(`Starsky mock app listening on port http://localhost:${port}`))

