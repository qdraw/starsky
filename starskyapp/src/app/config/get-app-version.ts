import { app } from "electron";

export function GetAppVersion(): string {
  if (process.env.npm_package_version) {
    return process.env.npm_package_version;
  }
  // in develop it returns the electron version
  return app.getVersion();
}
