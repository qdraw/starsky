import { app, ipcMain } from "electron";
import * as appConfig from "electron-settings";
import { AppVersionIpcKey } from "../config/app-version-ipc-key.const";
import { IlocationUrlSettings } from "../config/IlocationUrlSettings";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "../config/location-settings-ipc-keys.const";
import {
  LocationIsRemoteSettingsKey,
  LocationUrlSettingsKey
} from "../config/location-settings.const";

function ipcBridge() {
  ipcMain.on(LocationIsRemoteIpcKey, async (event, args) => {
    if (args !== undefined && args !== null) {
      await appConfig.set(LocationIsRemoteSettingsKey, args);
    }

    const currentSettings = await appConfig.get(LocationIsRemoteSettingsKey);

    let isLocationRemote = false;
    if (currentSettings !== undefined && currentSettings !== null) {
      isLocationRemote = currentSettings.toString() === "true";
    }

    console.log("-->", LocationIsRemoteIpcKey, isLocationRemote);

    event.reply(LocationIsRemoteIpcKey, isLocationRemote);
  });

  ipcMain.on(AppVersionIpcKey, async (event, args) => {
    const appVersion = app
      .getVersion()
      .match(new RegExp("^[0-9]+\\.[0-9]+", "ig"));

    console.log("-->", AppVersionIpcKey, appVersion);
    event.reply(AppVersionIpcKey, appVersion);
  });

  ipcMain.on(LocationUrlIpcKey, async (event, args: IlocationUrlSettings) => {
    // getting
    if (args === null) {
      const isRemote = (await appConfig.get(LocationIsRemoteIpcKey)) as boolean;

      const currentSettings = {
        location: await appConfig.get(LocationUrlSettingsKey)
      } as IlocationUrlSettings;

      if (!isRemote) {
        return;
      }
    }

    // console.log('000');

    // const currentSettings = {
    //     location: await appConfig.get(LocationUrlSettingsKey),
    // } as IlocationUrlSettings

    // if (args && !args.location.match(urlRegex) &&  !args.location.match(ipRegex)
    //     && !args.location.startsWith('http://localhost:') && args.location != currentSettings.location) {

    //     event.reply(LocationUrlIpcKey, {
    //        isValid: false
    //     } as IlocationUrlSettings);
    //     return;
    // }

    // if (args && args.location && ( args.location.match(urlRegex) || args.location.match(ipRegex)
    //     || args.location.startsWith('http://localhost:') )
    //     &&  args.location != currentSettings.location) {

    //     // to avoid errors
    //     var locationUrl = args.location.replace(/\/$/, "");

    //     const request = net.request({
    //         url: locationUrl + "/api/health",
    //         headers: {
    //             "Accept" :	"*/*"
    //         }
    //     } as any);

    //     request.on('response', async (response) => {
    //         console.log(`HEADERS: ${JSON.stringify(response.headers)} - ${response.statusCode} - ${locationUrl + "/api/health"}`)
    //         var locationOk = response.statusCode == 200 || response.statusCode == 503;
    //         if (locationOk) {
    //             currentSettings.location = locationUrl;
    //             // console.log('46', currentSettings);

    //             await appConfig.set(LocationUrlSettingsKey, currentSettings.location);
    //         }
    //         currentSettings.isValid = locationOk

    //         // to avoid that the session is opened
    //         mainWindows.forEach(window => {
    //             window.close()
    //         });

    //         event.reply('settings', currentSettings);

    //     });

    //     request.on('error',(e)=>{
    //         console.log(e);
    //         event.reply('settings', {...currentSettings,locationOk: false });
    //     })

    //     request.end()
    //     return;
    // }

    // event.reply(LocationUrlIpcKey, currentSettings )
  });
}

export default ipcBridge;
