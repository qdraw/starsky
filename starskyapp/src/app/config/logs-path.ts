import path = require("path");
import * as fs from "fs";
import { electronCacheLocation } from "../child-process/electron-cache-location";

export function MakeLogsPath() {
  var logFolder = path.join(electronCacheLocation(), "logs");
  if (!fs.existsSync(logFolder)) {
    fs.mkdirSync(logFolder);
  }
  return logFolder;
}
