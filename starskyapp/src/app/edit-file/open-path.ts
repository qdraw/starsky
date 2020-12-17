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
    // add extra test for photoshop
    if (
      overWriteDefaultApplication &&
      OsBuildKey() === "mac" &&
      overWriteDefaultApplication.includes("Adobe Photoshop")
    ) {
      const shouldRunFirst = await ShouldRunFirst();
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
  childProcess.exec(openWin);
}

function openMac(overWriteDefaultApplication: string, fullFilePath: string) {
  var openFileOnMac = `open -a "${overWriteDefaultApplication}" "${fullFilePath}"`;

  // // need to check if fullFilePath is directory
  childProcess.exec(openFileOnMac, {
    cwd: `${overWriteDefaultApplication}`
  });
}
