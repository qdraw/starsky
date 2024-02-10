import { app } from "electron";

export function isPackaged() :boolean {
  if (app === undefined) return false;
  return !!app.isPackaged;
}
