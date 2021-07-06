import { spawn } from "child_process";
import { app } from "electron";
import * as fs from "fs";
import * as getFreePort from "get-port";
import * as path from "path";
import * as readline from "readline";
import logger from "../logger/logger";
import { isPackaged } from "../os-info/is-packaged";
import { childProcessPath } from "./child-process-path";
import { electronCacheLocation } from "./electron-cache-location";

export let appPort: number = 9609;

export async function setupChildProcess() {
  var thumbnailTempFolder = path.join(
    electronCacheLocation(),
    "thumbnailTempFolder"
  );
  if (!fs.existsSync(thumbnailTempFolder)) {
    fs.mkdirSync(thumbnailTempFolder);
  }

  var tempFolder = path.join(electronCacheLocation(), "tempFolder");
  if (!fs.existsSync(tempFolder)) {
    fs.mkdirSync(tempFolder);
  }

  var appSettingsPath = path.join(electronCacheLocation(), "appsettings.json");
  var databaseConnection =
    "Data Source=" + path.join(electronCacheLocation(), "starsky.db");

  appPort = await getFreePort();

  const env = {
    ASPNETCORE_URLS: `http://localhost:${appPort}`,
    app__thumbnailTempFolder: thumbnailTempFolder,
    app__tempFolder: tempFolder,
    app__appSettingsPath: appSettingsPath,
    app__databaseConnection: databaseConnection,
    app__AccountRegisterDefaultRole: "Administrator",
    app__Verbose: !isPackaged() ? "true" : "false"
  };

  logger.info("env settings ->");
  logger.info(env);

  const appStarskyPath = childProcessPath();
  try {
    fs.chmodSync(appStarskyPath, 0o755);
  } catch (error) {
  }

  const starskyChild = spawn(appStarskyPath, {
    cwd: path.dirname(appStarskyPath),
    detached: true,
    env
  });

  starskyChild.stdout.on("data", function (data) {
    logger.info(data.toString());
    fs.appendFile(
      path.join(tempFolder, "child.log"),
      data.toString(),
      function (err) {}
    );
  });

  starskyChild.stderr.on("data", function (data) {
    logger.warn(data.toString());
    fs.appendFile(
      path.join(tempFolder, "child.log"),
      "stderr: " + data.toString(),
      function (err) {}
    );
  });

  readline.emitKeypressEvents(process.stdin);

  /**
   * Needed for terminals
   * @param {bool} modes true or false
   */
  function setRawMode(modes: boolean) {
    if (!process.stdin.setRawMode) return;
    process.stdin.setRawMode(modes);
  }

  function kill() {
    setRawMode(false);
    if (!starskyChild) return;
    starskyChild.stdin.end();
    // starskyChild.stdin.pause();
    starskyChild.kill();
  }

  setRawMode(true);

  process.stdin.on("keypress", (str, key) => {
    if (key.ctrl && key.name === "c") {
      kill();
      logger.info("=> (pressed ctrl & c) to the end of starsky");
      setTimeout(() => {
        process.exit(0);
      }, 400);
    }
  });

  app.on("before-quit", function (event) {
    event.preventDefault();
    logger.info("=> end default");
    kill();
    setTimeout(() => {
      process.exit(0);
    }, 400);
  });
}
