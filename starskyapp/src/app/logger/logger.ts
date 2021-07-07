import * as winston from "winston";
import { MakeTempPath } from "../config/temp-path";

const currentDate = new Date();
var today = currentDate.toISOString().split("T")[0];

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
      dirname: MakeTempPath(),
      filename: `${today}_combined.log`
    })
  ]
});

class logger {
  static info(message: any, ...meta: any[]) {
    if (!winstonLogger || !winstonLogger.info) {
      // keep console log here
      console.log(message, meta);
      return;
    }
    winstonLogger.info(message, meta);
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
