import * as os from "os";
import * as path from "path";
import { isPackaged } from "../os-info/is-packaged";
import OsBuildKey from "../os-info/os-build-key";

export function childProcessPath(): string {
  if (!isPackaged()) {
    // dev
    switch (process.platform) {
      case "darwin":
        // for: osx-arm64, osx-x64
        if (process.arch === "arm64") {
          return path.join(
            __dirname,
            "..",
            "..",
            "starsky",
            `osx-${process.arch}`,
            "starsky"
          );
        }
        return path.join(
          __dirname,
          "..",
          "..",
          "starsky",
          "osx-x64",
          "starsky"
        );
      case "win32":
        return path.join(
          __dirname,
          "..",
          "..",
          "starsky",
          "win-x64",
          "starsky.exe"
        );
      case "linux":
        return path.join(
          __dirname,
          "..",
          "..",
          "starsky",
          "linux-x64",
          "starsky"
        );
      default:
        throw new Error("os is not implemented");
    }
  }

  // osx-x64 or osx-arm64
  const targetFilePath = path.join(
    process.resourcesPath,
    "app",
    `${OsBuildKey()}-${os.arch()}`
  );

  let exeFilePath = path.join(targetFilePath, "starsky");
  // for windows
  if (process.platform === "win32") {
    exeFilePath = path.join(targetFilePath, "starsky.exe");
  }
  return exeFilePath;
}
