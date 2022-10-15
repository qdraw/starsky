import * as net from "net";

function createServer(port: number, callback?: () => void) {
  const server = net.createServer();
  server.listen(port, callback).unref();
}

// https://stackoverflow.com/a/54229665
export async function GetFreePort(): Promise<number> {
  return new Promise((resolve, _) => {
    createServer(0, () => {
      // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment, @typescript-eslint/no-unsafe-member-access
      const { port } = this.address();
      // eslint-disable-next-line @typescript-eslint/no-unsafe-argument, @typescript-eslint/restrict-plus-operands
      createServer(port + 1, () => {
        createServer(0, () => {
          // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment, @typescript-eslint/no-shadow, @typescript-eslint/no-unsafe-member-access
          const { port } = this.address();
          // This line will show that the OS skipped the occupied port and assigned the next available port.
          // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
          resolve(port);
        });
      });
    });
  });
}
