import { app } from "electron";
import util from "util";
import { terminateMainPid } from "../child-process/setup-child-process";
import logger from "../logger/logger";

export type BeforeQuitCallback = (ev?: Electron.Event) => Promise<void> | void;

export function beforeQuit(callback?: BeforeQuitCallback) {
  app.on("before-quit", async (event) => {
    // allow async cleanup by preventing default and then quitting afterwards
    try {
      event.preventDefault();
    } catch (err) {
      // some test mocks may not provide event.preventDefault
    }

    if (callback) {
      try {
        await callback(event);
      } catch (err) {
        // ignore cleanup errors
      }
    }

    // attempt to terminate child and then the main pid
    try {
      await terminateMainPid();
    } catch (err) {
      try {
        app.quit();
      } catch (_) {
        // no-op
      }
    }

    // If we still haven't exited after a short delay, log active handles
    // and force exit to avoid hanging indefinitely.
    setTimeout(() => {
      try {
        const getHandles = (process as any)._getActiveHandles;
        const getRequests = (process as any)._getActiveRequests;
        const handles = typeof getHandles === "function" ? getHandles.call(process) : [];
        const requests = typeof getRequests === "function" ? getRequests.call(process) : [];
        logger.info(`active handles on exit: ${util.inspect(handles, { depth: 2 })}`);
        logger.info(`active requests on exit: ${util.inspect(requests, { depth: 2 })}`);
      } catch (err) {
        logger.warn(`unable to enumerate active handles: ${err}`);
      }

      try {
        app.exit(0 as any);
      } catch (_) {
        try {
          process.exit(0);
        } catch (_) {
          // nothing else to do
        }
      }
    }, 2000);
  });

  process.stdin.on("keypress", (_, key: { ctrl: boolean, name: string }) => {
    if (key.ctrl && key.name === "c") {
      if (callback) {
        void Promise.resolve(callback()).then(() => terminateMainPid()).catch(() => terminateMainPid());
      } else {
        void terminateMainPid();
      }
    }
  });
}

export default beforeQuit;
