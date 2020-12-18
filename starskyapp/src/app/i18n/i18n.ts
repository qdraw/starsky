import { app } from "electron";

export function IsDutch(): boolean {
  return app.getLocale() === "nl";
}
