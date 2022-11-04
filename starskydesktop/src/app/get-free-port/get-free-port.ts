import * as net from "net";

export async function GetFreePort(): Promise<number> {
  return new Promise((resolve) => {
    const srv = net.createServer((sock) => {
      sock.end('Hello world\n');
    });
    srv.listen(0, () => {
      const { port } = srv.address() as net.AddressInfo;
      console.log(`Listening on port ${port}`);
      resolve(port);
    });
  });
}
