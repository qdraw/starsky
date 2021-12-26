import * as os from "os";
import * as path from "path";
/**
 * App Data folder
 * @returns AppData
 */
export function electronCacheLocation() {
  switch (process.platform) {
    case "darwin":
      // ~/Library/Application\ Support/starsky/Cache
      return path.join(
        os.homedir(),
        "Library",
        "ApplicationSupport",
        "starsky"
      );
    case "win32":
      // C:\Users\<user>\AppData\Roaming\starsky\Cache
      return path.join(os.homedir(), "AppData", "Roaming", "starsky");
    default:
      return path.join(os.homedir(), ".config", "starsky");
  }
}
