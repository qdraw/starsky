import * as childProcess from "child_process";
import { shell } from "electron";
import * as appConfig from "electron-settings";
import DefaultImageApplicationSetting from "../config/default-image-application-settings";
import logger from "../logger/logger";
import OsBuildKey from "../os-info/os-build-key";
import { IsApplicationRunning } from "./is-application-running";

/**
 * @see: https://community.adobe.com/t5/photoshop/problems-opening-photoshop-open-a/m-p/11541937?page=1
 * Since nobody cares
 */
async function ShouldRunFirst() {
  return IsApplicationRunning(".app/Contents/MacOS/Adobe\\ Photoshop");
}

function openWindows(
  overWriteDefaultApplication: string,
  fullFilePath: string
) {
  // need to check if fullFilePath is file
  const openWin = `"${overWriteDefaultApplication}" "${fullFilePath}"`;
  childProcess.exec(openWin);
}

function openMac(overWriteDefaultApplication: string, fullFilePath: string) {
  const openFileOnMac = `open -a "${overWriteDefaultApplication}" "${fullFilePath}"`;

  // // need to check if fullFilePath is directory
  childProcess.exec(openFileOnMac, {
    cwd: `${overWriteDefaultApplication}`
  });
}

export async function openPath(fullFilePath: string): Promise<void> {
  const overWriteDefaultApplication = (await appConfig.get(
    DefaultImageApplicationSetting
  )) as string;
  // eslint-disable-next-line @typescript-eslint/no-misused-promises
  return new Promise(async (resolve, reject) => {
    // add extra test for photoshop
    if (
      overWriteDefaultApplication
      && OsBuildKey() === "mac"
      && overWriteDefaultApplication.includes("Adobe Photoshop")
    ) {
      const shouldRunFirst = await ShouldRunFirst();
      if (!shouldRunFirst) {
        // eslint-disable-next-line prefer-promise-reject-errors
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
    logger.info("open default", fullFilePath);
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    shell.openPath(fullFilePath).then(() => {
      resolve();
    });
  });
}
