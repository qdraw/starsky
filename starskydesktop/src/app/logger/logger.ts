import * as path from "path";
import * as winston from "winston";
import { electronCacheLocation } from "../child-process/electron-cache-location";

const currentDate = new Date();
const today = currentDate.toISOString().split("T")[0].replace(/-/gi, "");

const winstonLogger = winston.createLogger({
  level: "info",
  format: winston.format.json(),
  defaultMeta: { service: "app" },
  transports: [
    new winston.transports.Console({
      level: "info",
    }),
    new winston.transports.Console({
      level: "warn",
    }),
    new winston.transports.File({
      dirname: path.join(electronCacheLocation(), "logs"),
      filename: `${today}_app_combined.log`,
    }),
  ],
});

// eslint-disable-next-line @typescript-eslint/naming-convention
class Logger {
  static info(message: unknown, ...meta: unknown[]) {
    try {
      // eslint-disable-next-line @typescript-eslint/no-unsafe-argument, @typescript-eslint/no-explicit-any
      winstonLogger.info(message as any, meta);
    } catch (error) {
      // keep console log here
      console.log(message, meta);
    }
  }

  static warn(message: any, ...meta: any[]) {
    try {
      // eslint-disable-next-line @typescript-eslint/no-unsafe-argument, @typescript-eslint/no-explicit-any
      winstonLogger.warn(message, meta);
    } catch (error) {
      // keep console log here
      console.log(message, meta);
    }
  }
}

export default Logger;
