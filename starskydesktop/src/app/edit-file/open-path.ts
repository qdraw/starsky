import { shell } from "electron";
import logger from "../logger/logger";

export async function OpenPath(fullFilePath: string): Promise<boolean> {
  return new Promise((resolve) => {
    logger.info("open default", fullFilePath);
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    shell.openPath(fullFilePath).then(() => {
      resolve(true);
    });
  });
}
