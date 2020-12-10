import { ipcMain } from "electron";
import { LocationUrlSettingsKey } from "../config/location-settings.const";
import * as appConfig from 'electron-settings'
import { ipRegex, urlRegex } from "../config/url-regex";
import { LocationIsRemoteIpcKey, LocationUrlIpcKey } from "../config/location-settings-ipc-keys.const";

function ipcBridge () {
    ipcMain.on(LocationIsRemoteIpcKey, (event, args) => {

    });
    ipcMain.on(LocationUrlIpcKey, async (event, args) => {
        console.log('000');
        
        var currentSettings = await appConfig.get(LocationUrlSettingsKey);

        if (args && !args.match(urlRegex) &&  !args.match(ipRegex) 
            && !args.startsWith('http://localhost:') && args != currentSettings) {

            event.reply(LocationUrlIpcKey, null);
            return;
        }


        event.reply(LocationUrlIpcKey, currentSettings)

    });
}

export default ipcBridge;