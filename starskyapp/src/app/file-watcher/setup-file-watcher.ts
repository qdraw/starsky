import * as chokidar from "chokidar";
import { createParentFolders } from "../edit-file/create-parent-folders";
import { GetParentDiskPath } from "../edit-file/get-parent-disk-path";
import { ActionWhenFileIsChanged } from "./action-when-file-is-changed";
import { FileWatcherObjects } from "./file-watcher.const";

export async function SetupFileWatcher() {
  FileWatcherObjects.forEach(([watch, path]) => {
    watch.removeAllListeners();
    FileWatcherObjects.delete([watch, path]);
    console.log("deleted:", path);
  });

  createParentFolders();
  const tempPathIncludingBaseUrl = await GetParentDiskPath();

  const watch = chokidar
    .watch(tempPathIncludingBaseUrl, {
      persistent: true,
      interval: 600,
      binaryInterval: 1200,
      alwaysStat: true
    })
    .on("change", (path, stats) => {
      ActionWhenFileIsChanged(path, stats);
    });

  FileWatcherObjects.add([watch, tempPathIncludingBaseUrl]);
  console.log("add:", tempPathIncludingBaseUrl);
}
