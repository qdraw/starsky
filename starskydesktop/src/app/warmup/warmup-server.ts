import waitOn from "wait-on";

export async function WarmupServer(
  port: number
): Promise<boolean> {
  const opts : waitOn.WaitOnOptions = {
    resources: [
      `http-get://localhost:${port}/api/health`
    ],
    delay: 1000, // initial delay in ms, default 0
    interval: 100, // poll interval in ms, default 250ms
    simultaneous: 1, // limit to 1 connection per resource at a time
    timeout: 10000, // timeout in ms, default Infinity
    tcpTimeout: 1000, // tcp timeout in ms, default 300ms
    window: 1000, // stabilization time in ms, default 750ms
    validateStatus(status : any) {
      return status === 200 || status === 503;
    },
    verbose: false,
  };
  // eslint-disable-next-line @typescript-eslint/no-misused-promises, no-async-promise-executor
  return new Promise((resolve) => {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    waitOn(opts)
      .then(() => {
        resolve(true);
      })
      .catch(() => {
        resolve(false);
      });
  });
}
