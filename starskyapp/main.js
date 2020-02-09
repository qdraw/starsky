// Modules to control application life and create native browser window
const { app, BrowserWindow } = require('electron')
const path = require('path')
const os = require('os');

function createWindow() {

  // to not have blinks in page navigation
  app.allowRendererProcessReuse = true;

  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 800,
    height: 600,
    frame: true,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js')
    }
  })

  // and load the index.html of the app.
  mainWindow.loadFile('index.html');


  // Open the DevTools.
  // mainWindow.webContents.openDevTools()
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', startApi)
app.on('ready', createWindow)

// Quit when all windows are closed.
app.on('window-all-closed', function () {
  // On macOS it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform !== 'darwin') app.quit()
})

app.on('activate', function () {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (BrowserWindow.getAllWindows().length === 0) createWindow()
})

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and require them here.
var apiProcess = null;

function startApi() {
  var proc = require('child_process').spawn;

  var apipath = "";
  var cwd = "";

  process.env['RANDOM_ID'] = Math.random();

  if (os.platform() === 'win32' && !app.isPackaged) {
    apipath = path.join(__dirname, '../', 'starsky', 'win7-x86', 'starsky.exe');
    cwd = path.join(__dirname, '../', 'starsky', 'win7-x86');
  }

  if (os.platform() === 'darwin' && !app.isPackaged) {
    apipath = path.join(__dirname, '../', 'starsky', 'osx.10.12-x64', 'starsky');
    cwd = path.join(__dirname, '../', 'starsky', 'osx.10.12-x64');
  }

  if (apipath === "" || cwd === "") {
    throw Error('platform not supported', apipath, cwd);
  }

  apiProcess = proc(apipath, {
    cwd
  })

  apiProcess.stdout.on('data', (data) => {
    console.log(`stdout: ${data}`);
  });
}

// Kill process when electron exits
process.on('exit', function () {
  console.log('exit');
  apiProcess.kill();
});