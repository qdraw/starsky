import { app, Menu } from "electron";
import { IsDutch } from "../i18n/i18n";
import createMainWindow from "../main-window/create-main-window";

export default function DockMenu() {
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
