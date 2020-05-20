// Modules to control application life and create native browser window
const { app } = require('electron')
const setStarskyPath = require('./lib/get-starsky-path').setStarskyPath
const URL = require('url').URL
const AppMenu = require('./lib/menu').AppMenu
const createWindow = require('./lib/create-window').createWindow
const ipcBridge = require('./lib/ipc-bridge').ipcBridge

app.allowRendererProcessReuse = true;

AppMenu();
setStarskyPath();
ipcBridge();

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


app.on('activate', (event, hasVisibleWindows) => {
  if (!hasVisibleWindows) { createWindow(); }
});