import { app, ipcMain } from "electron";
import { LocationIsRemoteSettingsKey, LocationUrlSettingsKey } from "../config/location-settings.const";
import * as appConfig from 'electron-settings'
import { ipRegex, urlRegex } from "../config/url-regex";
import { LocationIsRemoteIpcKey, LocationUrlIpcKey } from "../config/location-settings-ipc-keys.const";
import { AppVersionIpcKey } from "../config/app-version-ipc-key.const";

function ipcBridge () {
    ipcMain.on(LocationIsRemoteIpcKey, async (event, args) => {
        var currentSettings = await appConfig.get(LocationIsRemoteSettingsKey);

        console.log('-->', LocationIsRemoteIpcKey, currentSettings);
        event.reply(LocationIsRemoteIpcKey, currentSettings)
    });

    ipcMain.on(AppVersionIpcKey, async (event, args) => {
        const appVersion = app.getVersion().match(new RegExp("^[0-9]+\\.[0-9]+","ig"));

        console.log('-->', AppVersionIpcKey, appVersion);
        event.reply(AppVersionIpcKey, appVersion)
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