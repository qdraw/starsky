import { app } from "electron";

export function beforeQuit(callback: Function) {
  app.on("before-quit", function (event) {
    event.preventDefault();
    callback();
  });

  process.stdin.on("keypress", (str, key) => {
    if (key.ctrl && key.name === "c") {
      callback();
    }
  });
}
