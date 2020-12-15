// const {app, Menu, shell , BrowserWindow } = require('electron')
// const createMainWindow = require('./main-window').createMainWindow
// const createSettingsWindow = require('./settings-window').createSettingsWindow

import { app, BrowserWindow, Menu, shell } from "electron";
import { EditFile } from "../edit-file/edit-file";
import createMainWindow from "../main-window/create-main-window";
import { createSettingsWindow } from "../settings-window/create-settings-window";

function AppMenu() {
  const isMac = process.platform === "darwin";

  var menu = Menu.buildFromTemplate([
    ...(isMac
      ? [
          {
            label: app.name,
            submenu: [
              { role: "about" },
              { type: "separator" },
              { role: "services" },
              { type: "separator" },
              { role: "hide" },
              { role: "hideothers" },
              { role: "unhide" },
              { type: "separator" },
              { role: "quit" }
            ] as any
          }
        ]
      : []),
    {
      label: "File",
      submenu: [
        {
          label: "New Window",
          click: () => {
            createMainWindow();
          },
          accelerator: "CmdOrCtrl+N"
        },
        {
          label: "Edit file in Editor",
          click: () => {
            const focusWindow = BrowserWindow.getFocusedWindow();
            if (focusWindow) EditFile(focusWindow);
          },
          accelerator: "CmdOrCtrl+E"
        },

        isMac ? { role: "close" } : { role: "quit" }
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
        {
          label: "Select All",
          accelerator: "CmdOrCtrl+A",
          selector: "selectAll:"
        }
      ]
    },
    {
      label: "Settings",
      submenu: [
        {
          label: "Settings",
          click: () => {
            createSettingsWindow();
          },
          accelerator: "CmdOrCtrl+,"
        }
      ]
    },
    {
      label: "Develop",
      submenu: [
        {
          label: "Refresh",
          click: () => {
            BrowserWindow.getAllWindows().forEach((window) => {
              window.webContents.reload();
            });
          },
          accelerator: "CmdOrCtrl+R"
        },
        {
          label: "Dev Tools",
          click: () => {
            // only works on mac os
            BrowserWindow.getAllWindows().forEach((window) => {
              window.webContents.openDevTools();
            });
          },
          accelerator: "CmdOrCtrl+Alt+I"
        },
        {
          label: "Open in browser",
          click: async () => {
            await shell.openExternal(
              BrowserWindow.getFocusedWindow().webContents.getURL()
            );
          }
        }
      ]
    },
    {
      label: "View",
      submenu: [
        { role: "resetzoom" },
        { role: "zoomin" },
        { role: "zoomout" },
        { type: "separator" },
        { role: "togglefullscreen" }
      ]
    },
    {
      label: "Window",
      submenu: [
        { role: "minimize" },
        { role: "zoom" },
        ...(isMac
          ? [{ type: "separator" }, { role: "front" }]
          : [{ role: "close" }])
      ]
    },
    {
      role: "help",
      submenu: [
        {
          label: "Documentation website",
          click: async () => {
            await shell.openExternal("https://qdraw.github.io/starsky/");
          }
        },
        {
          label: "Release overview",
          // Referenced from HealthCheckForUpdates
          click: async () => {
            await shell.openExternal(
              "https://github.com/qdraw/starsky/releases/latest"
            );
          }
        }
      ]
    }
  ]);
  Menu.setApplicationMenu(menu);
}

export default AppMenu;
