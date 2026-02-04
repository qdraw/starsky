const express = require('express')
const app = express()

let port = 4000;

// argsparser
for (let i = 2; i < process.argv.length; i++){
  const current = process.argv[i];
  const prev = process.argv[i-1];
  
  if (current === "-h" || current === "--help" ) {
    console.log("Use arguments:")
    console.log(`--port ${port}`)
    process.exit(0);
  }

  if (process.argv.length <= 2) {
    continue;
  }
  if (prev === "--port") {
    port = current;
  }
}
// end argsparser

var bodyParser = require('body-parser');
app.use(bodyParser.urlencoded({ extended: true }));

var setRouter = require('./set-router.js').setRouter;

setRouter(app);

const http = require('http');
const server = http.createServer(app);

var setupServer = require('./websocket-server.js').setupServer;
setupServer(server);

// start our server
server.listen(port, () => {
    console.log(`Starsky mock app listening on port http://localhost:${port}`);
});

