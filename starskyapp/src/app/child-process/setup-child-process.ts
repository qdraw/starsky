import { spawn } from "child_process";
import { app } from "electron";
import * as fs from "fs";
import * as path from "path";
import * as readline from "readline";
import { isPackaged } from "../os-info/is-packaged";
import { childProcessPath } from "./child-process-path";
import { electronCacheLocation } from "./electron-cache-location";

export function setupChildProcess() {
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

  const env = {
    ASPNETCORE_URLS: "http://localhost:9609",
    app__thumbnailTempFolder: thumbnailTempFolder,
    app__tempFolder: tempFolder,
    app__appSettingsPath: appSettingsPath,
    app__databaseConnection: databaseConnection,
    app__AccountRegisterDefaultRole: "Administrator",
    app__Verbose: !isPackaged() ? "true" : "false"
  };

  console.log("env settings ->");
  console.log(env);

  const appStarskyPath = childProcessPath();
  fs.chmodSync(appStarskyPath, 0o755);

  const starskyChild = spawn(appStarskyPath, {
    cwd: path.dirname(appStarskyPath),
    detached: true,
    env
  });

  starskyChild.stdout.on("data", function (data) {
    console.log(data.toString());
  });

  starskyChild.stderr.on("data", function (data) {
    console.log("stderr: " + data.toString());
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
      console.log("===> end of starsky");
      setTimeout(() => {
        process.exit(0);
      }, 400);
    }
  });

  app.on("before-quit", function (event) {
    event.preventDefault();
    console.log("----> end");
    kill();
    setTimeout(() => {
      process.exit(0);
    }, 400);
  });
}
