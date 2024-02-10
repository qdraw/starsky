
import { createParentFolders } from "../edit-file/create-parent-folders";
import { GetParentDiskPath } from "../edit-file/get-parent-disk-path";
import logger from "../logger/logger";
import { Stats } from "fs";
import { ActionWhenFileIsChanged } from "./action-when-file-is-changed";
import { FileWatcherObjects } from "./file-watcher.const";
// eslint-disable-next-line @typescript-eslint/no-var-requires
const chokidar = require('chokidar');

export async function SetupFileWatcher() {
  FileWatcherObjects.forEach(([watch, path]) => {
    watch.removeAllListeners();
    FileWatcherObjects.delete([watch, path]);
    logger.info("[SetupFileWatcher] deleted:", path);
  });

  await createParentFolders();
  const tempPathIncludingBaseUrl = await GetParentDiskPath();

  const watch = chokidar
    .watch(tempPathIncludingBaseUrl, {
      persistent: true,
      interval: 600,
      binaryInterval: 1200,
      alwaysStat: true
    })
    .on("change", (path: string, stats: Stats) => {
      // eslint-disable-next-line @typescript-eslint/no-floating-promises
      ActionWhenFileIsChanged(path, stats);
    });

  FileWatcherObjects.add([watch, tempPathIncludingBaseUrl]);
  logger.info(`[SetupFileWatcher] add: ${tempPathIncludingBaseUrl}`);
}
