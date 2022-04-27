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

const webSocket = require('ws')
// initialize the WebSocket server instance
const wss = new webSocket.Server({ server });

wss.on('connection', (ws) => {

    //connection is up, let's add a simple simple event
    ws.on('message', (message) => {

        //log the received message and send it back to the client
        console.log('received: %s', message);
        ws.send(`Hello, you sent -> ${message}`);
    });

    setTimeout(()=>{
      console.log('--> welcome send');
      //send immediatly a feedback to the incoming connection    
      ws.send('{"data":{"dateTime":"2022-04-26T15:40:38.9756974Z"},"type":"Welcome"}');
    },100)

});

setInterval(() => {
    wss.clients.forEach((ws) => {
        if (!ws.isAlive) return ws.terminate();
        
        ws.isAlive = false;
        ws.ping(null, false, true);
        ws.send('{"data":{"speedInSeconds":30,"dateTime":"2022-04-26T15:41:05.2974217Z"},"type":"Heartbeat"}')
    });
}, 10000);

setInterval(() => {
    wss.clients.forEach((ws) => {
        ws.send('{"data":[{"filePath":"/0001","fileName":"0001","fileHash":null,"fileCollectionName":"0001","parentDirectory":"/","isDirectory":true,"tags":"0","status":"Ok","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":0,"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"unknown","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate"}')
    });
}, 1000);



// start our server
server.listen(port, () => {
    console.log(`Starsky mock app listening on port http://localhost:${port}`);
});

