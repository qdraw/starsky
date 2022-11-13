import * as os from "os";
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
          "..",
          "..",
          "starsky",
          "osx-x64",
          "starsky",
        );
      case "win32":
        return path.join(
          __dirname,
          "..",
          "..",
          "starsky",
          "win-x64",
          "starsky.exe",
        );
      case "linux":
        return path.join(
          __dirname,
          "..",
          "..",
          "starsky",
          "linux-x64",
          "starsky",
        );
      default:
        throw new Error("not implemented");
    }
  }

  // runtime-starsky-mac-x64
  const targetFilePath = path.join(
    process.resourcesPath,
    `runtime-starsky-${OsBuildKey()}-${os.arch()}`,
  );

  let exeFilePath = path.join(targetFilePath, "starsky");
  if (process.platform === "win32") { exeFilePath = path.join(targetFilePath, "starsky.exe"); }
  return exeFilePath;
}
