import { app } from "electron";

export function isPackaged() {
  return !!app.isPackaged;
}
