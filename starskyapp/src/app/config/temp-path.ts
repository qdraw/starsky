import { app } from "electron";
import * as fs from "fs";
import * as path from "path";
import logger from "../logger/logger";

export function TempPath(): string {
  try {
    return path.join(app.getPath("temp"), app.getName());
  } catch (error) {
    console.log("no temp path => ");
    return null;
  }
}

export function MakeTempPath(): string {
  const tempPath = TempPath();
  if (!tempPath) return;
  if (!fs.existsSync(tempPath)) {
    fs.mkdirSync(tempPath);
  }
  try {
    logger.info("tempPath => " + tempPath);
  } catch (error) {}

  return tempPath;
}
