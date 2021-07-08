import * as childProcess from "child_process";
import logger from "../logger/logger";

export const IsApplicationRunning = (query: string) => {
  return new Promise(function (resolve, reject) {
    let platform = process.platform;
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

    var starskyChild = childProcess.spawn(cmd, args);

    var stdOutData = "";

    starskyChild.stdout.on("data", function (stdout) {
      stdOutData += stdout.toString();
    });

    starskyChild.stdout.on("end", function () {
      var queryLowercaseNoEscape =
        "(grep )?" +
        query.toLowerCase().replace(/\\ /gi, " ").replace(/\//gi, ".");
      var matches = (
        stdOutData.match(new RegExp(queryLowercaseNoEscape, "ig")) || []
      ).filter((p) => p.indexOf("grep") === -1);
      resolve(matches.length >= 1);
    });

    starskyChild.stderr.on("data", function (data) {
      logger.info("IsApplicationRunning");
      logger.info("stderr: " + data.toString());
      reject();
    });
  });
};
