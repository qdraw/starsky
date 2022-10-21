import * as childProcess from "child_process";
import logger from "../logger/logger";

export const IsApplicationRunning = (query: string) => {
  return new Promise((resolve, reject) => {
    const { platform } = process;
    let cmd = "";
    let args: string[] = [];
    switch (platform) {
      case "win32":
        cmd = `tasklist`;
        break;
      case "darwin":
        cmd = "sh";
        args = ["-c", `ps aux | grep ${query}`];
        break;
      case "linux":
        cmd = "ps";
        args = ["-A"];
        break;
      default:
        break;
    }

    const starskyChild = childProcess.spawn(cmd, args);

    let stdOutData = "";

    starskyChild.stdout.on("data", (stdout) => {
      stdOutData += stdout.toString();
    });

    starskyChild.stdout.on("end", () => {
      const queryLowercaseNoEscape = `(grep )?${
        query.toLowerCase().replace(/\\ /gi, " ").replace(/\//gi, ".")}`;
      const matches = (
        stdOutData.match(new RegExp(queryLowercaseNoEscape, "ig")) || []
      ).filter((p) => p.indexOf("grep") === -1);
      resolve(matches.length >= 1);
    });

    starskyChild.stderr.on("data", (data) => {
      logger.info("IsApplicationRunning");
      logger.info(`stderr: ${data.toString()}`);
      reject();
    });
  });
};
