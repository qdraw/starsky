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

    // write an immediate synchronous dump so we always have a snapshot
    // even if later shutdown steps terminate the process quickly.
    try {
      const getHandles = (process as any)._getActiveHandles;
      const getRequests = (process as any)._getActiveRequests;
      const handles = typeof getHandles === "function" ? getHandles.call(process) : [];
      const requests = typeof getRequests === "function" ? getRequests.call(process) : [];
      const dump = {
        pid: process.pid,
        time: new Date().toISOString(),
        handles: handles.map((h: any) => ({ type: h && h.constructor ? h.constructor.name : typeof h })),
        requests: requests.map((r: any) => ({ type: r && r.constructor ? r.constructor.name : typeof r })),
      };
      const os = require("os");
      const fs = require("fs");
      const path = require("path");
      const tmp = path.join(os.tmpdir(), `starsky-exit-dump-${process.pid}-immediate.json`);
      try {
        fs.writeFileSync(tmp, JSON.stringify(dump, null, 2), { encoding: "utf8" });
        logger.info(`wrote immediate active-handle dump to: ${tmp}`);
      } catch (err) {
        logger.warn(`unable to write immediate dump file: ${err}`);
      }
    } catch (err) {
      logger.warn(`immediate dump failed: ${err}`);
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

    // If we still haven't exited after a short delay, write active handles
    // and requests to a temp file for inspection, then force exit.
    setTimeout(() => {
      try {
        const getHandles = (process as any)._getActiveHandles;
        const getRequests = (process as any)._getActiveRequests;
        const handles = typeof getHandles === "function" ? getHandles.call(process) : [];
        const requests = typeof getRequests === "function" ? getRequests.call(process) : [];
        const dump = {
          pid: process.pid,
          time: new Date().toISOString(),
          handles: handles.map((h: any) => ({ type: h && h.constructor ? h.constructor.name : typeof h })),
          requests: requests.map((r: any) => ({ type: r && r.constructor ? r.constructor.name : typeof r })),
        };
        const os = require("os");
        const fs = require("fs");
        const path = require("path");
        const tmp = path.join(os.tmpdir(), `starsky-exit-dump-${process.pid}.json`);
        try {
          fs.writeFileSync(tmp, JSON.stringify(dump, null, 2), { encoding: "utf8" });
          logger.info(`wrote active-handle dump to: ${tmp}`);
        } catch (err) {
          logger.warn(`unable to write dump file: ${err}`);
          logger.info(`active handles on exit: ${util.inspect(handles, { depth: 2 })}`);
          logger.info(`active requests on exit: ${util.inspect(requests, { depth: 2 })}`);
        }
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
