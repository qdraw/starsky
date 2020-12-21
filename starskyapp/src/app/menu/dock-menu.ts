import { app, Menu } from "electron";
import { IsDutch } from "../i18n/i18n";
import createMainWindow from "../main-window/create-main-window";
import OsBuildKey from "../os-info/os-build-key";

export default function DockMenu() {
  if (OsBuildKey() === "mac") {
    const dockMenu = Menu.buildFromTemplate([
      {
        label: IsDutch ? "Nieuw venster" : "New Window",
        click() {
          createMainWindow("?f=/");
        }
      }
    ]);
    app.dock.setMenu(dockMenu);
  }

  if (OsBuildKey() === "win") {
    app.setUserTasks([
      {
        program: process.execPath,
        arguments: "--new-window",
        iconPath: process.execPath,
        iconIndex: 0,
        title: IsDutch ? "Nieuw venster" : "New Window",
        description: IsDutch ? "Maak een nieuw venster" : "Create a new window"
      }
    ]);
  }
}
