import * as chokidar from "chokidar";

export function fileWatcher() {
  chokidar
    .watch(".", {
      persistent: true,
      interval: 300,
      binaryInterval: 600
    })
    .on("change", (path, stats) => {
      console.log(path, stats);
    });
}
