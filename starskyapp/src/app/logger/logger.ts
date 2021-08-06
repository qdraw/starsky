import * as winston from "winston";
import { electronCacheLocation } from "../child-process/electron-cache-location";
import path = require("path");

const currentDate = new Date();
var today = currentDate.toISOString().split("T")[0].replace(/-/gi, "");

const winstonLogger = winston.createLogger({
  level: "info",
  format: winston.format.json(),
  defaultMeta: { service: "app" },
  transports: [
    new winston.transports.Console({
      level: "info"
    }),
    new winston.transports.Console({
      level: "warn"
    }),
    new winston.transports.File({
      dirname: path.join(electronCacheLocation(), "logs"),
      filename: `${today}_app_combined.log`
    })
  ]
});

class logger {
  static info(message: any, ...meta: any[]) {
    try {
      winstonLogger.info(message, meta);
    } catch (error) {
      // keep console log here
      console.log(message, meta);
    }
  }
  static warn(message: any, ...meta: any[]) {
    try {
      winstonLogger.warn(message, meta);
    } catch (error) {
      // keep console log here
      console.log(message, meta);
    }
  }
}

export default logger;
