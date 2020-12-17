import { net, Session } from "electron";
import * as fs from "fs";

export function uploadNetRequest(
  url: string,
  toSubPath: string,
  fullFilePath: string,
  session: Session
): Promise<void> {
  return new Promise(function (resolve, reject) {
    console.log("> run upload " + url);

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
      if (response.statusCode !== 200)
        console.log(
          `HEADERS: ${JSON.stringify(response.headers)} - ${toSubPath} -  ${
            response.statusCode
          }`
        );

      if (response.statusCode !== 200) return;

      response.on("data", (chunk) => {
        body += chunk.toString();
      });
      response.on("end", () => {
        console.log(`BODY: ${body}`);
      });
    });

    // And now Upload
    fs.readFile(fullFilePath, function (err, data) {
      // skip error for now
      if (err) {
        reject(err);
        return;
      }

      request.write(data);
      request.end();
      request.on("finish", () => {
        console.log("--finish doUploadRequest");
        resolve();
      });
      request.on("error", (err) => {
        reject(err);
      });
    });
  });
}
