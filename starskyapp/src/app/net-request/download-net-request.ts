// import { net } from "electron";
// import path = require("path");

// export function downloadNetRequest(
//   fromMainWindow,
//   apiName,
//   parentFullFilePath,
//   formSubPath,
//   toSubPath
// ) {
//   if (!toSubPath) toSubPath = formSubPath;
//   return new Promise(function (resolve, reject) {
//     var fullFilePath = path.join(
//       parentFullFilePath,
//       new FileExtensions().GetFileName(toSubPath)
//     );

//     var file = fs.createWriteStream(fullFilePath);

//     const request = net.request({
//       useSessionCookies: true,
//       url:
//         getBaseUrlFromSettings() +
//         `/starsky/api/${apiName}?isThumbnail=false&f=${formSubPath}`,
//       session: fromMainWindow.webContents.session,
//       headers: {
//         Accept: "*/*"
//       }
//     });

//     request.on("response", (response) => {
//       console.log(
//         `api ${apiName} statusCode ${
//           response.statusCode
//         } - HEADERS: ${JSON.stringify(response.headers)}`
//       );

//       if (response.statusCode !== 200) {
//         console.log(response.statusCode);
//         reject(response.statusCode);
//         return;
//       }
//       response.pipe(file);

//       file.on("error", function (err) {
//         fs.unlink(dest); // Delete the file async. (But we don't check the result)
//         reject(err.message);
//       });

//       file.on("finish", function () {
//         fs.promises.stat(fullFilePath).then((stats) => {
//           if (response.headers["content-length"] === stats.size.toString()) {
//             resolve(fullFilePath);
//             return;
//           }
//           reject("byte size doesnt match");
//         });
//       });
//     });

//     // dont forget this one!
//     request.end();
//   });
// }
