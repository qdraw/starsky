/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import {
  app, BrowserWindow, Menu, shell
} from "electron";
import { EditFile } from "../edit-file/edit-file";
import { IsDutch } from "../i18n/i18n";
import createMainWindow from "../main-window/create-main-window";
import { createSettingsWindow } from "../settings-window/create-settings-window";

function AppMenu() {
  const isMac = process.platform === "darwin";

  const menu = Menu.buildFromTemplate([
    ...(isMac
      ? [
        {
          label: app.name,
          submenu: [
            {
              label: IsDutch() ? "Over Starsky" : "About Starsky",
              role: "about",
            },
            { type: "separator" },
            { role: "services" },
            { type: "separator" },
            {
              label: IsDutch() ? "Verberg Starsky" : "Hide Starsky",
              role: "hide",
            },
            {
              label: IsDutch() ? "Verberg andere" : "Hide Others",
              role: "hideothers",
            },
            {
              label: IsDutch() ? "Toon alles" : "Show All",
              role: "unhide",
            },
            { type: "separator" },
            {
              label: IsDutch() ? "Starsky afsluiten" : "Quit Starsky",
              role: "quit",
            },
          ] as any,
        },
      ]
      : []),
    {
      label: IsDutch() ? "Bestand" : "File",
      submenu: [
        {
          label: IsDutch() ? "Nieuw venster" : "New Window",
          click: () => {
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            createMainWindow("?f=/");
          },
          accelerator: "CmdOrCtrl+N",
        },
        {
          label: IsDutch() ? "Bewerk bestand in editor" : "Edit file in Editor",
          click: () => {
            const focusWindow = BrowserWindow.getFocusedWindow();
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            if (focusWindow) EditFile(focusWindow);
          },
          accelerator: "CmdOrCtrl+E",
        },

        isMac
          ? {
            label: IsDutch() ? "Venster sluiten" : "Close Window",
            role: "close",
          }
          : {
            label: IsDutch() ? "App sluiten" : "Close App",
            role: "quit",
          },
      ],
    },
    {
      label: IsDutch() ? "Bewerken" : "Edit",
      submenu: [
        {
          label: IsDutch() ? "Ongedaan maken" : "Undo",
          accelerator: "CmdOrCtrl+Z",
          selector: "undo:",
        },
        {
          label: IsDutch() ? "Opnieuw uitvoeren" : "Redo",
          accelerator: "Shift+CmdOrCtrl+Z",
          selector: "redo:",
        },
        { type: "separator" },
        {
          label: IsDutch() ? "Knippen" : "Cut",
          accelerator: "CmdOrCtrl+X",
          selector: "cut:",
        },
        {
          label: IsDutch() ? "Kopiëren" : "Copy",
          accelerator: "CmdOrCtrl+C",
          selector: "copy:",
        },
        {
          label: IsDutch() ? "Plakken" : "Paste",
          accelerator: "CmdOrCtrl+V",
          selector: "paste:",
        },
        {
          label: IsDutch() ? "Alles selecteren" : "Select All",
          accelerator: "CmdOrCtrl+A",
          selector: "selectAll:",
        },
      ],
    },
    {
      label: IsDutch() ? "Instellingen" : "Settings",
      submenu: [
        {
          label: IsDutch() ? "Instellingen" : "Settings",
          click: () => {
            // eslint-disable-next-line @typescript-eslint/no-floating-promises
            createSettingsWindow();
          },
          accelerator: "CmdOrCtrl+,",
        },
      ],
    },
    {
      label: IsDutch() ? "Ontwikkelaar" : "Develop",
      submenu: [
        {
          label: "Refresh",
          click: () => {
            BrowserWindow.getAllWindows().forEach((window) => {
              window.webContents.reload();
            });
          },
          accelerator: "CmdOrCtrl+R",
        },
        {
          label: "Dev Tools",
          click: () => {
            // only works on mac os
            BrowserWindow.getAllWindows().forEach((window) => {
              window.webContents.openDevTools();
            });
          },
          accelerator: "CmdOrCtrl+Alt+I",
        },
        {
          label: "Open in browser",
          // eslint-disable-next-line @typescript-eslint/no-misused-promises
          click: async () => {
            await shell.openExternal(BrowserWindow.getFocusedWindow().webContents.getURL());
          },
        },
      ],
    },
    {
      label: IsDutch() ? "Beeld" : "View",
      submenu: [
        {
          label: IsDutch() ? "Werkelijke grootte" : "Actual Size",
          role: "resetzoom",
        },
        {
          label: IsDutch() ? "Inzoomen" : "Zoom In",
          role: "zoomin",
        },
        {
          label: IsDutch() ? "Uitzoomen" : "Zoom Out",
          role: "zoomout",
        },
        { type: "separator" },
        {
          label: IsDutch() ? "Schermvullende weergave" : "Toggle Full Screen",
          role: "togglefullscreen",
        },
      ],
    },
    {
      label: IsDutch() ? "Venster" : "Window",
      submenu: [
        {
          label: IsDutch() ? "Minimaliseren" : "Minimize",
          role: "minimize",
        },
        { role: "zoom" },
        ...(isMac
          ? [{ type: "separator" }, { role: "front" }, { type: "separator" }, { role: "window" }]
          : [{ role: "close" }]),
      ],
    },
    {
      role: "help",
      submenu: [
        {
          label: "Documentation website",
          // eslint-disable-next-line @typescript-eslint/no-misused-promises
          click: async () => {
            await shell.openExternal("https://docs.qdraw.nl/docs/getting-started/first-steps");
          },
        },
        {
          label: "Release overview",
          // Referenced from HealthCheckForUpdates
          // eslint-disable-next-line @typescript-eslint/no-misused-promises
          click: async () => {
            await shell.openExternal("https://github.com/qdraw/starsky/releases/latest");
          },
        },
      ],
    },
  ]);
  Menu.setApplicationMenu(menu);
}

export default AppMenu;
