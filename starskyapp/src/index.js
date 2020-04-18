// Modules to control application life and create native browser window
const { app, BrowserWindow } = require('electron')
const path = require('path')
const portfinder = require('portfinder');
const { spawn } = require('child_process');
const URL = require('url').URL
const readline = require('readline');

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

var starskyChild = spawn('/data/git/starsky/starsky/osx.10.12-x64/starsky', {
  cwd: '/data/git/starsky/starsky/osx.10.12-x64',
  detached: true
}, (error, stdout, stderr) => { });

starskyChild.stdout.on('data', function (data) {
  console.log(data.toString());
});

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

setRawMode(true);

process.stdin.on('keypress', (str, key) => {
  if (key.ctrl && key.name === 'c') {
    starskyChild.stdin.pause();
    starskyChild.kill();
    setRawMode(false);
    console.log('===> end of starsky');
    setTimeout(() => { process.exit(0); }, 400);
  }
});

app.on("before-quit", function (event) {
  event.preventDefault();
  console.log('----> end');
  starskyChild.stdin.pause();
  starskyChild.kill();
  setRawMode(false);
  setTimeout(() => { process.exit(0); }, 400);
});



app.on('activate', function () {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (BrowserWindow.getAllWindows().length === 0) createWindow()
})

