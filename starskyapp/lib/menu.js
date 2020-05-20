const {app, Menu, shell ,remote } = require('electron')
const createWindow = require('./create-window').createWindow
const createSettingsWindow = require('./settings-window').createSettingsWindow

const mainWindows = require('./create-window').mainWindows
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
            createWindow()
          },
          accelerator: 'CmdOrCtrl+N'
        },
        isMac ? { role: 'close' } : { role: 'quit' },
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
        },
        {
          label: "Dev Tools",
          click: () => {
            mainWindows.forEach(window => {
              window.webContents.openDevTools()
            });
            settingsWindows.forEach(window => {
              window.webContents.openDevTools()
            });
          },
        },
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