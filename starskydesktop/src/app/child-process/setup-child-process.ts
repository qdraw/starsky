import { spawn } from "child_process";
import { app } from "electron";
import * as fs from "fs";
import * as path from "path";
import * as readline from "readline";
import { GetFreePort } from "../get-free-port/get-free-port";
import { SharedSettings } from "../global/global";
import logger from "../logger/logger";
import { isPackaged } from "../os-info/is-packaged";
import { childProcessPath } from "./child-process-path";
import { electronCacheLocation } from "./electron-cache-location";
import { SpawnCleanMacOs } from "./spawn-clean-mac-os";

function spawnChildProcess(appStarskyPath: string) {
  const starskyChild = spawn(appStarskyPath, {
    cwd: path.dirname(appStarskyPath),
    detached: true,
    env: process.env,
  });

  starskyChild.on("exit", (code) => {
    logger.info(`EXIT: CODE: ${code}`);
  });

  starskyChild.stdout.on("data", (data: string) => {
    logger.info(data.toString());
  });

  starskyChild.stderr.on("data", (data: string) => {
    logger.warn(data.toString());
  });

  return starskyChild;
}

function CreateTempThumbnailFolders() {
  const thumbnailTempFolder = path.join(electronCacheLocation(), "thumbnailTempFolder");
  if (!fs.existsSync(thumbnailTempFolder)) {
    fs.mkdirSync(thumbnailTempFolder);
  }

  const tempFolder = path.join(electronCacheLocation(), "tempFolder");
  if (!fs.existsSync(tempFolder)) {
    fs.mkdirSync(tempFolder);
  }
  return {
    tempFolder,
    thumbnailTempFolder,
  };
}

function EnvHelper(appPort: number) {
  const databaseConnection = `Data Source=${path.join(electronCacheLocation(), "starsky.db")}`;
  const appSettingsPath = path.join(electronCacheLocation(), "appsettings.json");
  const createTempThumbnailFolderResult = CreateTempThumbnailFolders();

  return {
    ASPNETCORE_URLS: `http://localhost:${appPort}`,
    app__thumbnailTempFolder: createTempThumbnailFolderResult.thumbnailTempFolder,
    app__tempFolder: createTempThumbnailFolderResult.tempFolder,
    app__appsettingspath: appSettingsPath,
    app__NoAccountLocalhost: "true",
    app__UseLocalDesktop: "true",
    app__databaseConnection: databaseConnection,
    app__ThumbnailGenerationIntervalInMinutes: !isPackaged() ? "-1" : "300",
    app__AccountRegisterDefaultRole: "Administrator",
    app__Verbose: !isPackaged() ? "true" : "false",
  };
}

export async function setupChildProcess() {
  const appSettingsPath = path.join(electronCacheLocation(), "appsettings.json");

  (global.shared as SharedSettings).port = await GetFreePort();

  logger.info(`next: port: ${(global.shared as SharedSettings).port}`);
  logger.info(`-appSettingsPath > ${appSettingsPath}`);

  const env = EnvHelper((global.shared as SharedSettings).port);
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

    SpawnCleanMacOs(appStarskyPath, process.platform)
      .then(() => {
        starskyChild = spawnChildProcess(appStarskyPath);
        starskyChild.addListener("close", () => {
          starskyChild = spawnChildProcess(appStarskyPath);
        });
      })
      .catch(() => {});
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
    starskyChild.kill();
  }

  setRawMode(true);

  process.stdin.on("keypress", (_, key: { name: string; ctrl: string }) => {
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
