import { net, Session } from "electron";
import * as fs from "fs";
import logger from "../logger/logger";

export function uploadNetRequest(
  url: string,
  toSubPath: string,
  fullFilePath: string,
  session: Session
): Promise<void> {
  return new Promise(function (resolve, reject) {
    if (url.startsWith("[object ")) {
      throw new Error("please await promise first");
    }
    logger.info("> run upload " + url);

    const request = net.request({
      useSessionCookies: true,
      url,
      session,
      method: "POST"
    });

    request.setHeader("to", toSubPath);
    request.setHeader("content-type", "application/octet-stream");
    request.setHeader("Accept", "*/*");

    // Reading response from API
    let body = "";
    request.on("response", (response) => {
      if (response.statusCode !== 200) {
        logger.info(
          `upload: ${response.statusCode} HEADERS: ${JSON.stringify(
            response.headers
          )} - ${toSubPath} `
        );
        // and end:
        return;
      }

      response.on("data", (chunk) => {
        body += chunk.toString();
      });
      response.on("end", () => {
        logger.info(`BODY: ${body}`);
      });
    });

    // And now Upload
    fs.readFile(fullFilePath, function (err, data) {
      // skip error for now
      if (err) {
        logger.info("skip due missing file: " + fullFilePath);
        reject(err);
        return;
      }

      request.write(data);
      request.end();
      request.on("finish", () => {
        logger.info("--finish doUploadRequest " + fullFilePath);
        fs.promises.stat(fullFilePath).then((stat) => {
          fs.promises.writeFile(fullFilePath + ".info", stat.size.toString());
        });
        resolve();
      });
      request.on("error", (err) => {
        logger.info("error doUploadRequest " + fullFilePath);
        logger.info(err);
        reject(err);
      });
    });
  });
}
