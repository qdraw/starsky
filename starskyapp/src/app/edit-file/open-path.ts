import * as childProcess from "child_process";
import { shell } from "electron";
import * as appConfig from "electron-settings";
import DefaultImageApplicationSetting from "../config/default-image-application-settings";
import OsBuildKey from "../os-info/os-build-key";
import { IsApplicationRunning } from "./is-application-running";

export async function openPath(fullFilePath: string): Promise<void> {
  const overWriteDefaultApplication = (await appConfig.get(
    DefaultImageApplicationSetting
  )) as string;
  return new Promise(async function (resolve, reject) {
    console.log(overWriteDefaultApplication);

    // add extra test for photoshop
    if (
      overWriteDefaultApplication &&
      OsBuildKey() === "mac" &&
      overWriteDefaultApplication.includes("Adobe Photoshop")
    ) {
      const shouldRunFirst = await ShouldRunFirst();
      console.log(shouldRunFirst);

      if (!shouldRunFirst) {
        reject(
          "Photoshop is not running, please start photoshop first and try it again"
        );
        return;
      }
    }

    if (overWriteDefaultApplication && OsBuildKey() === "mac") {
      openMac(overWriteDefaultApplication, fullFilePath);
      resolve();
      return;
    }

    if (overWriteDefaultApplication && OsBuildKey() === "win") {
      openWindows(overWriteDefaultApplication, fullFilePath);
      resolve();
      return;
    }

    console.log("open default", fullFilePath);
    shell.openPath(fullFilePath).then((_) => {
      resolve();
    });
  });
}

/**
 * @see: https://community.adobe.com/t5/photoshop/problems-opening-photoshop-open-a/m-p/11541937?page=1
 */
async function ShouldRunFirst() {
  return await IsApplicationRunning(".app/Contents/MacOS/Adobe\\ Photoshop");
}

function openWindows(
  overWriteDefaultApplication: string,
  fullFilePath: string
) {
  // need to check if fullFilePath is file
  var openWin = `"${overWriteDefaultApplication}" "${fullFilePath}"`;
  console.log(openWin);
  childProcess.exec(openWin);
}

function openMac(overWriteDefaultApplication: string, fullFilePath: string) {
  var openFileOnMac = `open -a "${overWriteDefaultApplication}" "${fullFilePath}"`;
  console.log("openFileOnMac " + openFileOnMac);

  // // need to check if fullFilePath is directory
  childProcess.exec(openFileOnMac, {
    cwd: `${overWriteDefaultApplication}`
  });
}

// export function openPath11(fullFilePath: string) {
//   return new Promise(function (resolve, reject) {
//     if (appConfig.has("settings_default_app") && osType.getOsKey() === "mac") {
//       function excuteOpenFileOnMac() {
//         var openFileOnMac = `open -a "${appConfig.get(
//           "settings_default_app"
//         )}" "${fullFilePath}"`;
//         console.log("openFileOnMac " + openFileOnMac);
//         // // need to check if fullFilePath is directory
//         childProcess.exec(openFileOnMac, {
//           cwd: `${appConfig.get("settings_default_app")}`,
//           shell: true
//         });
//         resolve();
//       }

//       // There are issues oping photoshop
//       // see: https://community.adobe.com/t5/photoshop/problems-opening-photoshop-open-a/m-p/11541937?page=1
//       if (
//         appConfig.get("settings_default_app").indexOf("Adobe Photoshop.app")
//       ) {
//         isRunning(".app/Contents/MacOS/Adobe\\ Photoshop", (isRunning) => {
//           if (!isRunning) {
//             console.log("not running");
//             reject(
//               "Photoshop is not running, please start photoshop first and try it again"
//             );
//             return;
//           }
//           excuteOpenFileOnMac();
//         });
//         return;
//       }

//       excuteOpenFileOnMac();
//     } else if (
//       appConfig.has("settings_default_app") &&
//       osType.getOsKey() === "win"
//     ) {
//       // need to check if fullFilePath is file
//       var openWin = `"${appConfig.get(
//         "settings_default_app"
//       )}" "${fullFilePath}"`;
//       console.log(openWin);
//       childProcess.exec(openWin);
//       resolve();
//     } else {
//       console.log("open default", fullFilePath);
//       shell.openPath(fullFilePath).then((_) => {
//         resolve();
//       });
//     }
//   });
// }
