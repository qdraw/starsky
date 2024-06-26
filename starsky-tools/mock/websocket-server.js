
const webSocket = require('ws');

function addLeadingZeros(num, totalLength = 2) {
  return String(num).padStart(totalLength, '0');
}

function setupServer(server) {
  // initialize the WebSocket server instance
  const wss = new webSocket.Server({ server });

  function heartbeat() {
    this.isAlive = true;
  }

  wss.on('connection', function connection(ws) {

        //connection is up, let's add a simple simple event
      ws.on('message', (message) => {

          //log the received message and send it back to the client
          console.log('received: %s', message);
          ws.send(`Hello, you sent -> ${message}`);
      });

    ws.isAlive = true;
    ws.on('pong', heartbeat);
  });

  const interval = setInterval(function ping() {
    wss.clients.forEach(function each(ws) {
      if (ws.isAlive === false) return ws.terminate();

      ws.isAlive = false;
      ws.ping();
      ws.send('{"data":{"speedInSeconds":30,"dateTime":"2022-04-26T15:41:05.2974217Z"},"type":"Heartbeat"}')
    });
  }, 30000);

  setInterval(() => {
      wss.clients.forEach((ws) => {
          const dateObject =new Date();
          const tags = `${addLeadingZeros(dateObject.getHours())}:${addLeadingZeros(dateObject.getMinutes())}:${addLeadingZeros(dateObject.getSeconds())}`
          const randomColorClass = Math.floor(Math.random() * 4);
          ws.send(`{"data":[{"filePath":"/0001","fileName":"0001","fileHash":null,"fileCollectionName":"0001","parentDirectory":"/","isDirectory":true,"tags":"${tags}","status":"Ok","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":${randomColorClass},"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"unknown","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate"}`)
      });
  }, 1000);

  setInterval(() => {
      wss.clients.forEach((ws) => {
          const dateObject =new Date();
          const tags = `${addLeadingZeros(dateObject.getHours())}:${addLeadingZeros(dateObject.getMinutes())}:${addLeadingZeros(dateObject.getSeconds())}`
          const randomColorClass = Math.floor(Math.random() * 4);
          
          ws.send(`{"data":[{"filePath":"/0001/test","fileName":"test","fileHash":null,"fileCollectionName":"test","parentDirectory":"/0001","isDirectory":true,"tags":"${tags}","status":"Ok","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":${randomColorClass},"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"unknown","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate-0001-test"}`)
          ws.send(`{"data":[{"filePath":"/0001/image.jpg","fileName":"image.jpg","fileHash":null,"fileCollectionName":"image","parentDirectory":"/0001","isDirectory":false,"tags":"${tags}","status":"Ok","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":0,"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"unknown","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate-0001-image"}`)

          // remove and create new item
          const randomBoolean = Math.random() < 0.5;
          if (randomBoolean) {
            // Delete
            ws.send(`{"data":[{"filePath":"/0002","fileName":"0002","fileHash":null,"fileCollectionName":"0002","parentDirectory":"/","isDirectory":true,"tags":"${tags}","status":"Deleted","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":0,"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"unknown","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate-0002"}`)
            ws.send(`{"data":[{"filePath":"/0001/test2","fileName":"test2","fileHash":null,"fileCollectionName":"test2","parentDirectory":"/0001","isDirectory":true,"tags":"${tags}","status":"Deleted","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":0,"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"unknown","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate-0001-test2"}`)
            ws.send(`{"data":[{"filePath":"/0001/toggleDeleted.jpg","fileName":"toggleDeleted","fileHash":null,"fileCollectionName":"toggleDeleted","parentDirectory":"/0001","isDirectory":false,"tags":"${tags}","status":"Deleted","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":0,"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"unknown","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate-0001-toggleDeleted"}`)
            ws.send(`{"data":[{"filePath":"/0003.xmp","fileName":"0003.xmp","fileHash":null,"fileCollectionName":"0003","parentDirectory":"/","isDirectory":false,"tags":"${tags}","status":"Deleted","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":0,"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"xmp","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate-0003-xmp"}`)
          }
          else {
            // Ok
            ws.send(`{"data":[{"filePath":"/0002","fileName":"0002","fileHash":null,"fileCollectionName":"0002","parentDirectory":"/","isDirectory":true,"tags":"${tags}","status":"Ok","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":0,"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"unknown","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate-0002"}`)
            ws.send(`{"data":[{"filePath":"/0001/test2","fileName":"test2","fileHash":null,"fileCollectionName":"test2","parentDirectory":"/0001","isDirectory":true,"tags":"${tags}","status":"Ok","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":0,"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"unknown","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate-0001-test2"}`)
            ws.send(`{"data":[{"filePath":"/0001/toggleDeleted.jpg","fileName":"toggleDeleted","fileHash":null,"fileCollectionName":"toggleDeleted","parentDirectory":"/0001","isDirectory":false,"tags":"${tags}","status":"Ok","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":0,"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"unknown","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate-0001-toggleDeleted"}`)
            ws.send(`{"data":[{"filePath":"/0003.xmp","fileName":"0003.xmp","fileHash":null,"fileCollectionName":"0003","parentDirectory":"/","isDirectory":false,"tags":"${tags}","status":"Ok","description":"","title":"","dateTime":"0001-01-01T00:00:00","addToDatabase":"2022-02-13T17:04:56.268554","lastEdited":"2022-04-26T15:47:40.9316578Z","latitude":0,"longitude":0,"locationAltitude":0,"locationCity":"","locationState":"","locationCountry":"","colorClass":0,"orientation":"DoNotChange","imageWidth":0,"imageHeight":0,"imageFormat":"xmp","collectionPaths":[],"sidecarExtensionsList":[],"aperture":0,"shutterSpeed":"","isoSpeed":0,"software":"","makeModel":"","make":"","model":"","lensModel":"","focalLength":0,"size":0,"imageStabilisation":0}],"type":"MetaUpdate-0003-xmp"}`)
          }

        });

  }, 3000);

  wss.on('close', function close() {
    clearInterval(interval);
  });
}

module.exports = {
  setupServer
};