import { app } from "electron";
import * as fs from "fs";
import * as path from "path";
import logger from "../logger/logger";

export function TempPath(): string {
  return path.join(app.getPath("temp"), app.getName());
}

export function MakeTempPath(): string {
  const tempPath = TempPath();
  if (!fs.existsSync(tempPath)) {
    fs.mkdirSync(tempPath);
  }
  logger.info("tempPath => " + tempPath);

  return tempPath;
}
