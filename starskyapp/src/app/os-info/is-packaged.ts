import { app } from "electron";

export function isPackaged() {
  if (app === undefined) return false;
  return !!app.isPackaged;
}
