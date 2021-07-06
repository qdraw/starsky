import * as winston from 'winston';
import { MakeTempPath } from "../config/temp-path";

const currentDate = new Date();
var today = currentDate.toISOString().split('T')[0]

const logger = winston.createLogger({
  levels: {
    info: 2,
    warn: 3,
  },
  transports: [
    new(winston.transports.Console)({
      level: "info",
    }),
    new(winston.transports.Console)({
      level: "warn",
    }),
    new (winston.transports.File)({ 
      filename: `${today}_info.log`,
      dirname: MakeTempPath(),
      level: "info",
    }),
    new (winston.transports.File)({ 
      filename: `${today}_warn.log`,  
      dirname: MakeTempPath(),
      level: "warn",
    })
  ]
})

export default logger;