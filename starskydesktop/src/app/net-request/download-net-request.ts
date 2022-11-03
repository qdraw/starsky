import { net } from "electron";
import * as fs from "fs";
import logger from "../logger/logger";

export function downloadNetRequest(
  url: string,
  session: Electron.Session,
  toPath: string
) {
  return new Promise((resolve, reject) => {
    if (url.startsWith("[object ")) {
      throw new Error("please await promise first");
    }

    const file = fs.createWriteStream(toPath);
    logger.info(`> request: ${url} -> ${toPath}`);

    const request = net.request({
      useSessionCookies: true,
      url,
      session
    });

    request.setHeader("Accept", "/*");

    request.on("response", (response) => {
      if (response.statusCode !== 200) {
        logger.info(response.statusCode);
        reject(response.statusCode);
        return;
      }

      fs.writeFileSync(
        `${toPath}.info`,
        response.headers["content-length"]?.toString()
      );

      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      response.pipe(file);

      file.on("error", (err) => {
        fs.unlink(toPath, () => { }); // Delete the file async. (But we don't check the result)
        reject(err.message);
      });

      file.on("finish", () => {
        // eslint-disable-next-line @typescript-eslint/no-floating-promises
        fs.promises.stat(toPath).then((stats) => {
          if (response.headers["content-length"] === stats.size.toString()) {
            resolve(toPath);
            return;
          }
          fs.unlinkSync(`${toPath}.info`);
          // eslint-disable-next-line prefer-promise-reject-errors
          reject("byte size doesnt match");
        });
      });
    });

    // dont forget this one!
    request.end();
  });
}
