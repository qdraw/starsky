import net = require('net');

// https://stackoverflow.com/a/54229665
export async function GetFreePort(): Promise<number> {

  return new Promise(function (resolve, _) {
    createServer(0, function () {
      const port = this.address().port;
      createServer(port+1, function () {
        createServer(0, function () {
          const port = this.address().port;
          // This line will show that the OS skipped the occupied port and assigned the next available port.
          resolve(port);
        });
      });
    });
  });
}


function createServer(port: number, callback?: () => void) {
  const server = net.createServer();
  server.listen(port, callback).unref();
}