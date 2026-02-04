import { app } from "electron";

// eslint-disable-next-line @typescript-eslint/ban-types
export function beforeQuit(callback: Function) {
  app.on("before-quit", (event) => {
    event.preventDefault();
    callback();
  });

  process.stdin.on("keypress", (str, key : { ctrl: boolean, name:string }) => {
    if (key.ctrl && key.name === "c") {
      callback();
    }
  });
}

export default beforeQuit;
