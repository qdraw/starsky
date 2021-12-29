import * as path from "path";
import { isPackaged } from "../os-info/is-packaged";
import OsBuildKey from "../os-info/os-build-key";

export function childProcessPath(): string {
  if (!isPackaged()) {
    // dev
    switch (process.platform) {
      case "darwin":
        return path.join(
          __dirname,
          "../",
          "../",
          "../",
          "../",
          "starsky",
          "osx-x64",
          "starsky"
        );
      case "win32":
        return path.join(
          __dirname,
          "../",
          "../",
          "../",
          "../",
          "starsky",
          "win7-x64",
          "starsky.exe"
        );
      default:
        throw new Error("not implemented");
    }
  }

  var targetFilePath = path.join(
    process.resourcesPath,
    `runtime-starsky-${OsBuildKey()}`
  );

  var exeFilePath = path.join(targetFilePath, "starsky");
  if (process.platform === "win32")
    exeFilePath = path.join(targetFilePath, "starsky.exe");
  return exeFilePath;
}
