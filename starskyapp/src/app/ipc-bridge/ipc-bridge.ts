import { ipcMain } from "electron";
import { LocationIsRemote, LocationUrl } from "../config/location-settings.const";

function ipcBridge () {
    ipcMain.on(LocationIsRemote, (event, args) => {

    });
    ipcMain.on(LocationUrl, (event, args) => {

    });
}

export default ipcBridge;