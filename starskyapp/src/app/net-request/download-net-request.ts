import { net } from "electron";
import * as fs from "fs";

export function downloadNetRequest(
  url: string,
  session: Electron.Session,
  toPath: string
) {
  return new Promise(function (resolve, reject) {
    if (url.startsWith("[object ")) {
      throw new Error("please await promise first");
    }

    var file = fs.createWriteStream(toPath);

    const request = net.request({
      useSessionCookies: true,
      url,
      session
    });

    request.setHeader("Accept", "/*");

    request.on("response", (response) => {
      // console.log(
      //   `${url} statusCode ${response.statusCode} - HEADERS: ${JSON.stringify(
      //     response.headers
      //   )}`
      // );

      if (response.statusCode !== 200) {
        console.log(response.statusCode);
        reject(response.statusCode);
        return;
      }

      fs.promises.writeFile(
        toPath + ".info",
        response.headers["content-length"].toString()
      );

      // @ts-ignore
      response.pipe(file);

      file.on("error", function (err) {
        fs.unlink(toPath, () => {}); // Delete the file async. (But we don't check the result)
        reject(err.message);
      });

      file.on("finish", function () {
        fs.promises.stat(toPath).then(async (stats) => {
          if (response.headers["content-length"] === stats.size.toString()) {
            resolve(toPath);
            return;
          }
          fs.promises.unlink(toPath + ".info");
          reject("byte size doesnt match");
        });
      });
    });

    // dont forget this one!
    request.end();
  });
}
