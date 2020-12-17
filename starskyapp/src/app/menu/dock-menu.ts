import { app, Menu } from "electron";
import createMainWindow from "../main-window/create-main-window";

export default function DockMenu() {
  const dockMenu = Menu.buildFromTemplate([
    {
      label: "New Window",
      click() {
        createMainWindow("?f=/");
      }
    }
  ]);
  app.dock.setMenu(dockMenu);
}
