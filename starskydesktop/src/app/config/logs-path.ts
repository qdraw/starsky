import * as path from "path";
import * as fs from "fs";
import { electronCacheLocation } from "../child-process/electron-cache-location";

export function MakeLogsPath() {
  const logFolder = path.join(electronCacheLocation(), "logs");
  if (!fs.existsSync(logFolder)) {
    fs.mkdirSync(logFolder);
  }
  return logFolder;
}
