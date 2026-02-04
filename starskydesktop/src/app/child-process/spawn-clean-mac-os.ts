import { spawn } from "child_process";
import * as path from "path";
import logger from "../logger/logger";

export function ExecuteXattrCommand(
  appStarskyPath: string,
  xattr: string = "xattr"
): Promise<void> {
  return new Promise((resolve, reject) => {
    const xattrArgs = ["-rd", "com.apple.quarantine", appStarskyPath];
    const xattrOptions = {
      detached: true,
      env: process.env,
      argv0: xattr,
    };

    const xattrChild = spawn(xattr, xattrArgs, xattrOptions);

    xattrChild.on("error", (err) => {
      logger.info("Error occurred while running xattr command:", err);
      reject(err);
    });

    xattrChild.on("exit", (code, signal) => {
      if (code === 0) {
        logger.info("xattr command completed successfully.");
        resolve();
      } else {
        logger.info(`xattr command exited with code ${code} and signal ${signal}`);
        reject(new Error(`xattr command exited with code ${code} and signal ${signal}`));
      }
    });
  });
}

export function ExecuteCodesignCommand(
  appStarskyPath: string,
  codesign: string = "codesign"
): Promise<void> {
  return new Promise((resolve, reject) => {
    const args = [
      "--force",
      "--deep",
      "-s",
      "-", // Assuming this is the identity flag for the signing certificate
      appStarskyPath,
    ];

    logger.info(`appStarskyPath: ${appStarskyPath} - codesign: ${codesign} -- `);

    const options = {
      cwd: path.dirname(appStarskyPath),
      detached: true,
      env: process.env,
      argv0: codesign,
    };

    const codeSignSpawn = spawn(codesign, args, options);

    codeSignSpawn.on("exit", (code) => {
      logger.info(`code sign EXIT: CODE: ${code}`);
      if (code === 0) {
        resolve();
      } else {
        reject(new Error(`codesign command exited with code ${code}`));
      }
    });

    codeSignSpawn.stdout.on("data", (data: string) => {
      logger.info(data.toString());
    });

    codeSignSpawn.stderr.on("data", (data: string) => {
      logger.warn(data.toString());
    });
  });
}

export function SpawnCleanMacOs(appStarskyPath: string, processPlatform: string): Promise<boolean> {
  return new Promise((resolve, reject) => {
    if (processPlatform !== "darwin") {
      resolve(true);
      return;
    }

    Promise.all([ExecuteXattrCommand(appStarskyPath), ExecuteCodesignCommand(appStarskyPath)])
      .then(() => {
        resolve(true);
      })
      .catch((err: Error) => {
        reject(err);
      });
  });
}
