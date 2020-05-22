// Modules to control application life and create native browser window
const { app } = require('electron')
const setupChildProcess = require('./lib/setup-child-process').setupChildProcess
const URL = require('url').URL
const AppMenu = require('./lib/app-menu').AppMenu
const createMainWindow = require('./lib/main-window').createMainWindow
const ipcBridge = require('./lib/ipc-bridge').ipcBridge
const appConfig = require('electron-settings');

app.allowRendererProcessReuse = true;

AppMenu();
setupChildProcess();
ipcBridge();

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.whenReady().then(createMainWindow)

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

    // for example the upgrade page
    if (navigationUrl.startsWith('file://')) {
      return;      
    }

    // to allow remote connections
    var currentSettings = appConfig.get("settings");
    if (currentSettings && currentSettings.remote && currentSettings.location && parsedUrl.origin.startsWith(new URL(currentSettings.location).origin)) {
      return;
    }
    

    if (!parsedUrl.origin.startsWith('http://localhost:')) {
      event.preventDefault()
    }
  })
})


app.on('activate', (event, hasVisibleWindows) => {
  if (!hasVisibleWindows) { createMainWindow(); }
});