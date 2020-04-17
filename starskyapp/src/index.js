// Modules to control application life and create native browser window
const {app, BrowserWindow} = require('electron')
const path = require('path')
const portfinder = require('portfinder');
const { spawn } = require('child_process');

app.allowRendererProcessReuse = true;

function createWindow () {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js')
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
}, (error, stdout, stderr) => {
  if (error) {
    console.error(`exec error: ${error}`);
    return;
  }
  console.log(`stdout: ${stdout}`);
  console.error(`stderr: ${stderr}`);
});


// function searchPort() {
//   portfinder.getPortPromise()
//     .then((port) => {
//
//
//     })
//     .catch((err) => {
//         //
//         // Could not get a free port, `err` contains the reason.
//         //
//     });
// }

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

 app.on('will-quit' , function () {
   process.kill(-starskyChild.pid);
 });

app.on('activate', function () {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (BrowserWindow.getAllWindows().length === 0) createWindow()
})

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and require them here.
