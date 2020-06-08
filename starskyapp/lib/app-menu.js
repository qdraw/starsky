const {app, Menu, shell , BrowserWindow } = require('electron')
const createMainWindow = require('./main-window').createMainWindow
const createSettingsWindow = require('./settings-window').createSettingsWindow

const mainWindows = require('./main-window').mainWindows
const settingsWindows = require('./settings-window').settingsWindows

function AppMenu() {
  const isMac = process.platform === 'darwin';
  var menu = Menu.buildFromTemplate([
    ...(isMac ? [{
      label: app.name,
      submenu: [
        { role: 'about' },
        { type: 'separator' },
        { role: 'services' },
        { type: 'separator' },
        { role: 'hide' },
        { role: 'hideothers' },
        { role: 'unhide' },
        { type: 'separator' },
        { role: 'quit' }
      ]
    }] : []),
    {
      label: 'File',
      submenu: [
        {
          label: "New Window",
          click: () => {
            createMainWindow()
          },
          accelerator: 'CmdOrCtrl+N'
        },
        isMac ? { role: 'close' } : { role: 'quit' },
      ]
    },
    {
      label: "Edit",
      submenu: [
          { label: "Undo", accelerator: "CmdOrCtrl+Z", selector: "undo:" },
          { label: "Redo", accelerator: "Shift+CmdOrCtrl+Z", selector: "redo:" },
          { type: "separator" },
          { label: "Cut", accelerator: "CmdOrCtrl+X", selector: "cut:" },
          { label: "Copy", accelerator: "CmdOrCtrl+C", selector: "copy:" },
          { label: "Paste", accelerator: "CmdOrCtrl+V", selector: "paste:" },
          { label: "Select All", accelerator: "CmdOrCtrl+A", selector: "selectAll:" }
      ]
    },
    {
      label: 'Settings',
      submenu: [
        {
          label: "Remote settings",
          click: () => {
            createSettingsWindow()
          },
          accelerator: 'CmdOrCtrl+,'
        },
      ]
    },
    {
      label: 'Develop',
      submenu: [
        {
          label: "Refresh",
          click: () => {
            mainWindows.forEach(window => {
              window.webContents.reload()
            });
            settingsWindows.forEach(window => {
              window.webContents.reload()
            });
          },
          accelerator: 'CmdOrCtrl+R'
        },
        {
          label: "Dev Tools",
          click: () => {
            BrowserWindow.getAllWindows().forEach(window => {
              window.webContents.openDevTools()
            });
          },
          accelerator: 'CmdOrCtrl+Alt+I'
        },
        {
          label: "Open in browser",
          click: async () => {
            await shell.openExternal(BrowserWindow.getFocusedWindow().webContents.getURL())
          }
        },
      ]
    },
    {
      label: 'View',
      submenu: [
        { role: 'resetzoom' },
        { role: 'zoomin' },
        { role: 'zoomout' },
        { type: 'separator' },
        { role: 'togglefullscreen' }
      ]
    },
    {
      label: 'Window',
      submenu: [
        { role: 'minimize' },
        { role: 'zoom' },
        ...(isMac ? [
          { type: 'separator' },
          { role: 'front' },
        ] : [
          { role: 'close' }
        ])
      ]
    },
    {
      role: 'help',
      submenu: [
        {
          label: 'Learn More',
          click: async () => {
            await shell.openExternal('https://qdraw.github.io/starsky/')
          }
        }
      ]
    }
  ])
  Menu.setApplicationMenu(menu); 
}

module.exports = {
  AppMenu
}