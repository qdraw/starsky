import { spawn } from "child_process";
import { app } from "electron";
import * as fs from "fs";
import * as path from "path";
import * as readline from "readline";
import util from "util";
import { GetFreePort } from "../get-free-port/get-free-port";
import { SharedSettings } from "../global/global";
import logger from "../logger/logger";
import { isPackaged } from "../os-info/is-packaged";
import { childProcessPath } from "./child-process-path";
import { electronCacheLocation } from "./electron-cache-location";
import { SpawnCleanMacOs } from "./spawn-clean-mac-os";

// Hold a reference to the currently active async cleanup function so
// other modules can await child-process shutdown.
let _killChildRef: (() => Promise<void>) | null = null;

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

  // when detached, allow the child to continue independently and
  // let the parent exit without waiting on the child's event loop
  if (typeof starskyChild.unref === "function") {
    try {
      starskyChild.unref();
    } catch (err) {
      logger.warn(`unable to unref child process: ${err}`);
    }
  }

  return starskyChild;
}

/**
 * Terminate main process after ensuring child is cleaned up.
 * Skips actual kill in test environment to avoid aborting the test runner.
 */
export async function terminateMainPid(): Promise<void> {
  if (process.env.JEST_WORKER_ID || process.env.NODE_ENV === "test") {
    logger.info("skip terminating main pid in test environment");
    return;
  }

  try {
    if (_killChildRef) {
      await _killChildRef();
    }
  } catch (err) {
    logger.warn(`error while waiting for child cleanup: ${err}`);
  }

  try {
    logger.info(`requesting app.quit() to terminate main process: ${process.pid}`);
    try {
      app.quit();
    } catch (e) {
      logger.warn(`app.quit() failed: ${e}`);
    }
    // ensure exit after short delay if quit didn't terminate
    setTimeout(() => {
      try {
        logger.info("calling app.exit() fallback to force exit");
        app.exit(0 as any);
      } catch (err) {
        try {
          process.exit(0);
        } catch (_) {
          // nothing else we can do
        }
      }
    }, 1500);
  } catch (err) {
    logger.warn(`unexpected error while attempting to quit: ${err}`);
  }
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
  const appSettingsLocalPath = path.join(electronCacheLocation(), "appsettings.local.json");
  const createTempThumbnailFolderResult = CreateTempThumbnailFolders();

  return {
    ASPNETCORE_URLS: `http://localhost:${appPort}`,
    app__thumbnailTempFolder: createTempThumbnailFolderResult.thumbnailTempFolder,
    app__tempFolder: createTempThumbnailFolderResult.tempFolder,
    app__appsettingspath: appSettingsPath,
    app__appsettingslocalpath: appSettingsLocalPath,
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
  const appSettingsLocalPath = path.join(electronCacheLocation(), "appsettings.local.json");

  (global.shared as SharedSettings).port = await GetFreePort();

  logger.info(`next: port: ${(global.shared as SharedSettings).port}`);
  logger.info(`-appSettingsPath > ${appSettingsPath}`);
  logger.info(`-appSettingsLocalPath > ${appSettingsLocalPath}`);

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
  let isShuttingDown = false;

  function attachCloseHandler() {
    starskyChild.addListener("close", () => {
      if (isShuttingDown) {
        logger.info("skip restart process (shutting down)");
        return;
      }
      logger.info("restart process");

      SpawnCleanMacOs(appStarskyPath, process.platform)
        .then(() => {
          if (isShuttingDown) {
            logger.info("skip spawn process (shutting down)");
            return;
          }
          starskyChild = spawnChildProcess(appStarskyPath);
          attachCloseHandler();
        })
        .catch(() => { });
    });
  }

  attachCloseHandler();

  readline.emitKeypressEvents(process.stdin);

  /**
   * Needed for terminals
   * @param {bool} modes true or false
   */
  function setRawMode(modes: boolean) {
    if (!process.stdin.setRawMode) return;
    process.stdin.setRawMode(modes);
  }

  const keypressHandler = (_: unknown, key: { name: string; ctrl: boolean }) => {
    if (key.ctrl && key.name === "c") {
      // perform async cleanup then quit
      void killChild()
        .then(() => {
          logger.info("=> (pressed ctrl & c) to the end of starsky");
          app.quit();
        })
        .catch(() => app.quit());
    }
  };

  function kill() {
    isShuttingDown = true;
    try {
      if (typeof process.stdin.removeListener === "function") {
        process.stdin.removeListener("keypress", keypressHandler);
      }
    } catch (err) {
      logger.warn(`unable to remove keypress listener: ${err}`);
    }

    try {
      setRawMode(false);
    } catch (err) {
      logger.warn(`unable to set raw mode false: ${err}`);
    }

    if (!starskyChild) return;

    logger.info(`killing child pid: ${starskyChild.pid}`);
    if (starskyChild.stdin) {
      try {
        starskyChild.stdin.end();
      } catch (err) {
        logger.warn(`unable to end child stdin: ${err}`);
      }
    }

    try {
      // attempt graceful termination
      starskyChild.kill("SIGTERM");
    } catch (error) {
      logger.warn(`unable to send SIGTERM to child process: ${error}`);
    }

    // if still alive after short timeout, force kill
    setTimeout(() => {
      try {
        // check if process exists
        if (starskyChild && starskyChild.pid) {
          process.kill(starskyChild.pid, 0);
          logger.info(`child still alive after SIGTERM, sending SIGKILL to pid: ${starskyChild.pid}`);
          try {
            process.kill(starskyChild.pid, "SIGKILL");
          } catch (err) {
            logger.warn(`unable to SIGKILL child process: ${err}`);
          }
        }
      } catch (err) {
        // process.kill(pid,0) throws if not exists => no action needed
      }
    }, 1000);
  }

  function killChild(): Promise<void> {
    return new Promise((resolve) => {
      isShuttingDown = true;

      try {
        if (typeof process.stdin.removeListener === "function") {
          process.stdin.removeListener("keypress", keypressHandler);
        }
      } catch (err) {
        logger.warn(`unable to remove keypress listener: ${err}`);
      }

      try {
        setRawMode(false);
      } catch (err) {
        logger.warn(`unable to set raw mode false: ${err}`);
      }

      if (!starskyChild) return resolve();

      logger.info(`killing child pid: ${starskyChild.pid}`);
      if (starskyChild.stdin) {
        try {
          starskyChild.stdin.end();
        } catch (err) {
          logger.warn(`unable to end child stdin: ${err}`);
        }
      }

      try {
        // attempt graceful termination
        starskyChild.kill("SIGTERM");
      } catch (error) {
        logger.warn(`unable to send SIGTERM to child process: ${error}`);
      }

      let finished = false;
      const onExit = () => {
        if (finished) return;
        finished = true;
        resolve();
      };

      try {
        if (typeof starskyChild.once === "function") {
          starskyChild.once("exit", onExit);
        } else if (typeof starskyChild.on === "function") {
          starskyChild.on("exit", onExit);
        } else {
          // no exit event support on this mock, resolve after timeout
        }
      } catch (err) {
        // ignore
      }

      // fallback force-kill after short timeout
      setTimeout(() => {
        if (finished) return;
        try {
          if (starskyChild && starskyChild.pid) {
            process.kill(starskyChild.pid, 0);
            logger.info(`child still alive after SIGTERM, sending SIGKILL to pid: ${starskyChild.pid}`);
            try {
              process.kill(starskyChild.pid, "SIGKILL");
            } catch (err) {
              logger.warn(`unable to SIGKILL child process: ${err}`);
            }
          }
        } catch (err) {
          // process not running
        }
        if (!finished) {
          finished = true;
          resolve();
        }
      }, 1200);
    });
  }

  // expose current cleanup function to module scope so other callers
  // (for example a terminate helper) can await child shutdown.
  _killChildRef = killChild;

  if (process.stdin.isTTY) {
    setRawMode(true);
    process.stdin.on("keypress", keypressHandler);
  }

  app.on("before-quit", () => {
    logger.info("=> end default");
    void (async () => {
      try {
        await killChild();
        logger.info("=> kill done sub process");
      } catch (err) {
        logger.warn(`error during killChild in before-quit: ${err}`);
      }

      // log active handles/requests right after child cleanup to help
      // identify what's keeping the main event loop alive.
      try {
        const getHandles = (process as any)._getActiveHandles;
        const getRequests = (process as any)._getActiveRequests;
        const handles = typeof getHandles === "function" ? getHandles.call(process) : [];
        const requests = typeof getRequests === "function" ? getRequests.call(process) : [];
        logger.info(`post-kill active handles: ${util.inspect(handles, { depth: 2 })}`);
        logger.info(`post-kill active requests: ${util.inspect(requests, { depth: 2 })}`);
      } catch (err) {
        logger.warn(`unable to enumerate active handles: ${err}`);
      }

      try {
        logger.info("calling terminateMainPid from setup-child-process before-quit");
        await terminateMainPid();
      } catch (err) {
        logger.warn(`terminateMainPid failed: ${err}`);
      }
    })();
  });
}
