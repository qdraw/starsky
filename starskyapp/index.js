// Modules to control application life and create native browser window
const { app, BrowserWindow } = require('electron')
const path = require('path')
const { spawn } = require('child_process');
const URL = require('url').URL
const readline = require('readline');
const extract = require('extract-zip');
const fs = require('fs');

app.allowRendererProcessReuse = true;

function createWindow() {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      enableRemoteModule: false,
      partition: 'persist:main',
      // preload: path.join(__dirname, 'preload.js')
    }
  })

  // and load the index.html of the app.
  mainWindow.loadFile('index.html')

  // Open the DevTools.
  // mainWindow.webContents.openDevTools()
}

function isPackaged() {
  return !!app.isPackaged;
}

function getStarskyPath() {

  console.log(process.platform);

  console.log(!isPackaged());
  if (!isPackaged()) { // dev
    var exeFilePath = "";
    switch (process.platform) {
      case "darwin":
        return Promise.resolve(path.join(__dirname, "../", "starsky", "osx.10.12-x64", "starsky"));
      case "win32":
        return Promise.resolve(path.join(__dirname, "../", "starsky", "win7-x86", "starsky"));
      default:
        return Promise.resolve(exeFilePath);
    }
  }

  // prod

  var includedZipPath = path.join(process.resourcesPath, `include-starsky-${process.platform}.zip`);
  var targetFilePath = path.join(process.resourcesPath, "include");

  var exeFilePath = path.join(targetFilePath, "starsky")
  if (process.platform === "win32") exeFilePath = path.join(targetFilePath, "starsky.exe");

  return new Promise(function (resolve, reject) {
    fs.promises.access(exeFilePath).then((status) => {
      console.log(status);
      resolve(exeFilePath);
    }).catch(() => {
      extract(includedZipPath, { dir: targetFilePath }).then(() => {

        // make chmod +x
        if (process.platform !== "win32") fs.chmodSync(exeFilePath, 0o755);
        resolve(exeFilePath);
      }).catch((error) => {
        console.log('catch', error);
      });
    });
  });

}

var starskyChild;
getStarskyPath().then((starskyPath) => {
  starskyChild = spawn(starskyPath, {
    cwd: path.dirname(starskyPath),
    detached: true,
    env: {
      "ASPNETCORE_URLS": "http://localhost:9609"
    }
  }, (error, stdout, stderr) => { });

  starskyChild.stdout.on('data', function (data) {
    console.log(data.toString());
  });
})

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.whenReady().then(createWindow)

// Quit when all windows are closed.
app.on('window-all-closed', function () {
  // On macOS it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform !== 'darwin') app.quit()
})

// https://github.com/electron/electron/blob/master/docs/tutorial/security.md
app.on('web-contents-created', (event, contents) => {
  contents.on('will-navigate', (event, navigationUrl) => {
    const parsedUrl = new URL(navigationUrl)
    if (!parsedUrl.origin.startsWith('http://localhost:')) {
      event.preventDefault()
    }
  })
})

readline.emitKeypressEvents(process.stdin);

/**
 * Needed for terminals
 * @param {bool} modes true or false
 */
function setRawMode(modes) {
  if (!process.stdin.setRawMode) return;
  process.stdin.setRawMode(modes)
}

function kill() {
  setRawMode(false);
  if (!starskyChild) return;
  starskyChild.stdin.pause();
  starskyChild.kill();
}

setRawMode(true);

process.stdin.on('keypress', (str, key) => {
  if (key.ctrl && key.name === 'c') {
    kill();
    console.log('===> end of starsky');
    setTimeout(() => { process.exit(0); }, 400);
  }
});

app.on("before-quit", function (event) {
  event.preventDefault();
  console.log('----> end');
  kill();
  setTimeout(() => { process.exit(0); }, 400);
});


app.on('activate', function () {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (BrowserWindow.getAllWindows().length === 0) createWindow()
})
