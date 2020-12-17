import * as chokidar from "chokidar";
import { GetParentDiskPath } from "../edit-file/get-parent-disk-path";
import { FileWatcherObjects } from "./file-watcher.const";

export async function SetupFileWatcher() {
  FileWatcherObjects.forEach(([watch, path]) => {
    watch.removeAllListeners();
    FileWatcherObjects.delete([watch, path]);
  });

  const tempPathIncludingBaseUrl = await GetParentDiskPath();

  const watch = chokidar
    .watch(tempPathIncludingBaseUrl, {
      persistent: true,
      interval: 600,
      binaryInterval: 1200
    })
    .on("change", (path, stats) => {
      console.log(path, stats);
    });

  FileWatcherObjects.add([watch, tempPathIncludingBaseUrl]);
}
