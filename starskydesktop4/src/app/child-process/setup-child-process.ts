import { spawn } from "child_process";
import { app } from "electron";
import * as fs from "fs";
import * as path from "path";
import * as readline from "readline";
import { GetFreePort } from "../get-free-port/get-free-port";
import logger from "../logger/logger";
import { isPackaged } from "../os-info/is-packaged";
import { childProcessPath } from "./child-process-path";
import { electronCacheLocation } from "./electron-cache-location";

// eslint-disable-next-line import/no-mutable-exports
export let appPort = 9609;

function spawnChildProcess(appStarskyPath: string) {
  
  logger.info(`next: appStarskyPath: ${appStarskyPath}`)

  const starskyChild = spawn(appStarskyPath, {
    cwd: path.dirname(appStarskyPath),
    detached: true,
    env: process.env,
  });

  starskyChild.stdout.on("data", (data: string) => {
    logger.info(data.toString());
  });

  starskyChild.stderr.on("data", (data: string) => {
    logger.warn(data.toString());
  });

  return starskyChild;
}

export async function setupChildProcess() {
  const thumbnailTempFolder = path.join(electronCacheLocation(), "thumbnailTempFolder");
  if (!fs.existsSync(thumbnailTempFolder)) {
    fs.mkdirSync(thumbnailTempFolder);
  }

  const tempFolder = path.join(electronCacheLocation(), "tempFolder");
  if (!fs.existsSync(tempFolder)) {
    fs.mkdirSync(tempFolder);
  }

  const appSettingsPath = path.join(electronCacheLocation(), "appsettings.json");
  const databaseConnection = `Data Source=${path.join(electronCacheLocation(), "starsky.db")}`;

  appPort = await GetFreePort();

  logger.info(`next: port: ${appPort}`);
  logger.info(`-appSettingsPath > ${appSettingsPath}`);

  const env = {
    ASPNETCORE_URLS: `http://localhost:${appPort}`,
    app__thumbnailTempFolder: thumbnailTempFolder,
    app__tempFolder: tempFolder,
    app__appsettingspath: appSettingsPath,
    app__NoAccountLocalhost: "true",
    app__UseLocalDesktopUi: "true",
    app__databaseConnection: databaseConnection,
    app__AccountRegisterDefaultRole: "Administrator",
    app__Verbose: !isPackaged() ? "true" : "false",
  };
  process.env = { ...process.env, ...env };

  logger.info("env settings ->");
  logger.info(env);
  logger.info(`app data folder -> ${path.join(app.getPath("appData"), "starsky")}`);

  const appStarskyPath = childProcessPath();

  try {
    fs.chmodSync(appStarskyPath, 0o755);
  } catch (error) {
    // do nothing
  }

  let starskyChild = spawnChildProcess(appStarskyPath);

  starskyChild.addListener("close", () => {
    logger.info("restart process");
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    GetFreePort().then((p) => {
      appPort = p;
      env.ASPNETCORE_URLS = `http://localhost:${appPort}`;
      logger.info(`next: port: ${appPort}`);
      starskyChild = spawnChildProcess(appStarskyPath);
    });
  });

  readline.emitKeypressEvents(process.stdin);

  function kill() {
    if (!starskyChild) return;
    starskyChild.stdin.end();
    starskyChild.kill();
  }

  process.stdin.on("keypress", (_, key: { name: string; ctrl: string }) => {
    console.log(key.name);
    
    if (key.ctrl && key.name === "c") {
      kill();
      logger.info("=> (pressed ctrl & c) to the end of starsky");
      setTimeout(() => {
        process.exit(0);
      }, 400);
    }
  });

  app.on("before-quit", (event) => {
    event.preventDefault();
    logger.info("=> end default");
    kill();
    setTimeout(() => {
      process.exit(0);
    }, 400);
  });
}
