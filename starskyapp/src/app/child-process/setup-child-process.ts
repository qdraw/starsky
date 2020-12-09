import { app } from "electron";
import { electronCacheLocation } from "./electron-cache-location";
import * as fs from 'fs'
import * as path from 'path'
import {spawn} from 'child_process'
import { childProcessPath } from "./child-process-path";
import * as readline from 'readline';
import { isPackaged } from "../os-info/is-packaged";

export function setupChildProcess() {

  var thumbnailTempFolder = path.join(electronCacheLocation(),"thumbnailTempFolder");
  if (!fs.existsSync(thumbnailTempFolder)) {
      fs.mkdirSync(thumbnailTempFolder)
  }

  var tempFolder = path.join(electronCacheLocation(), "tempFolder");
  if (!fs.existsSync(tempFolder)) {
      fs.mkdirSync(tempFolder)
  }

  // when exiftool already exist
  const exifToolPath = path.join(tempFolder,"exiftool-unix", "exiftool");
  if (fs.existsSync(exifToolPath)) {
    const fd = fs.openSync(exifToolPath, "r");
    fs.fchmodSync(fd, 0o777);
  }

  var appSettingsPath = path.join(electronCacheLocation(), "appsettings.json");
  var databaseConnection = "Data Source=" + path.join(electronCacheLocation(), "starsky.db") ;
  
  console.log({
    "ASPNETCORE_URLS": "http://localhost:9609",
    "app__thumbnailTempFolder": thumbnailTempFolder,
    "app__tempFolder": tempFolder,
    "app__appSettingsPath" : appSettingsPath,
    "app__databaseConnection": databaseConnection,
    "app__AccountRegisterDefaultRole": "Administrator",
    "app__Verbose": !isPackaged() ? "true" : "false",
  });
  
  const appStarskyPath = childProcessPath();

  const starskyChild = spawn(appStarskyPath, {
    cwd: path.dirname(appStarskyPath),
    detached: true,
    env: {
      "ASPNETCORE_URLS": "http://localhost:9609",
      "app__thumbnailTempFolder": thumbnailTempFolder,
      "app__tempFolder": tempFolder,
      "app__appSettingsPath" : appSettingsPath,
      "app__databaseConnection": databaseConnection,
      "app__AccountRegisterDefaultRole": "Administrator",
      "app__Verbose": !isPackaged() ? "true" : "false",
    }
  });

  starskyChild.stdout.on('data', function (data) {
    console.log(data.toString());
  });

  starskyChild.stderr.on('data', function (data) {
    console.log('stderr: ' + data.toString());
  });

  readline.emitKeypressEvents(process.stdin);

  /**
   * Needed for terminals
   * @param {bool} modes true or false
   */
  function setRawMode(modes: boolean) {
      if (!process.stdin.setRawMode) return;
      process.stdin.setRawMode(modes)
  }

  function kill() {
    setRawMode(false);
    if (!starskyChild) return;
    starskyChild.stdin.end();
      // starskyChild.stdin.pause();
    starskyChild.kill();
  }

  setRawMode(true);

  process.stdin.on('keypress', (str, key) => {
    if (key.ctrl && key.name === 'c') {
      kill();
      console.log('===> end of starsky');
      setTimeout(() => { 
        process.exit(0); 
      }, 400);
    }
  });

  app.on("before-quit", function (event) {
    event.preventDefault();
    console.log('----> end');
    kill();
    setTimeout(() => { 
      process.exit(0); 
    }, 400);
  });
}