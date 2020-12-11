import { app, ipcMain, net } from "electron";
import * as appConfig from "electron-settings";
import { AppVersionIpcKey } from "../config/app-version-ipc-key.const";
import { IlocationUrlSettings } from "../config/IlocationUrlSettings";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "../config/location-ipc-keys.const";
import {
  LocationIsRemoteSettingsKey,
  LocationUrlSettingsKey
} from "../config/location-settings.const";
import { UpdatePolicyIpcKey } from "../config/update-policy-ipc-key.const";
import { UpdatePolicySettings } from "../config/update-policy-settings.const";
import { ipRegex, urlRegex } from "../config/url-regex";
import { mainWindows } from "../main-window/main-windows.const";

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

  ipcMain.on(LocationUrlIpcKey, async (event, args: string) => {
    // getting
    if (!args) {
      const isRemote = (await appConfig.get(
        LocationIsRemoteSettingsKey
      )) as boolean;

      const currentSettings = {
        location: await appConfig.get(LocationUrlSettingsKey),
        isValid: null,
        isLocal: false
      } as IlocationUrlSettings;

      console.log(isRemote, currentSettings.location);

      if (!isRemote || !currentSettings.location) {
        event.reply(LocationUrlIpcKey, {
          isValid: null,
          isLocal: true,
          location: "http://localhost:9609"
        } as IlocationUrlSettings);
        return;
      }

      event.reply(LocationUrlIpcKey, currentSettings);
      return;
    }

    if (
      args.match(urlRegex) ||
      args.match(ipRegex) ||
      args.startsWith("http://localhost:")
    ) {
      // to avoid errors
      var locationUrl = args.replace(/\/$/, "");
      const request = net.request({
        url: locationUrl + "/api/health",
        headers: {
          Accept: "*/*"
        }
      } as any);

      request.on("response", async (response) => {
        console.log(
          `HEADERS: ${JSON.stringify(response.headers)} - ${
            response.statusCode
          } - ${locationUrl + "/api/health"}`
        );

        const responseSettings = {
          location: locationUrl,
          isLocal: false
        } as IlocationUrlSettings;

        var locationOk =
          response.statusCode == 200 || response.statusCode == 503;
        if (locationOk) {
          await appConfig.set(LocationUrlSettingsKey, locationUrl);
        }

        responseSettings.isValid = locationOk;

        // to avoid that the session is opened
        mainWindows.forEach((window) => {
          window.close();
        });

        event.reply(LocationUrlIpcKey, responseSettings);
      });

      request.on("error", (e) => {
        console.log(e);
        event.reply(LocationUrlIpcKey, {
          isValid: false,
          isLocal: false,
          location: args
        } as IlocationUrlSettings);
      });

      request.end();
      return;
    }

    event.reply(LocationUrlIpcKey, {
      isValid: false,
      isLocal: false,
      location: args
    } as IlocationUrlSettings);
  });

  // ipcMain.on("settings_default_app", (event, args) => {
  //   if (args && args.reset) {
  //     appConfig.delete("settings_default_app");
  //     event.reply("settings_default_app", "");
  //     return;
  //   }

  //   if (args && args.showOpenDialog) {
  //     var newOpenedWindow = new BrowserWindow();
  //     var selected = dialog.showOpenDialog(newOpenedWindow, {
  //       properties: ["openFile"]
  //     });

  //     selected
  //       .then((data) => {
  //         if (data.canceled) {
  //           newOpenedWindow.close();
  //           return;
  //         }
  //         appConfig.set("settings_default_app", data.filePaths[0]);
  //         event.reply("settings_default_app", data.filePaths[0]);
  //         newOpenedWindow.close();
  //       })
  //       .catch((e) => {
  //         newOpenedWindow.close();
  //       });
  //   }

  //   if (appConfig.has("settings_default_app")) {
  //     var currentSettings = appConfig.get("settings_default_app");
  //     event.reply("settings_default_app", currentSettings);
  //   }
  // });

  // ipcMain.on("settings_update_policy", (event, args) => {
  //   let currentSettings = true;

  //   if (appConfig.has("settings_update_policy")) {
  //     currentSettings = appConfig.get("settings_update_policy");
  //   }

  //   if (args === false || args === true) {
  //     console.log("set arg --> ", args);
  //     appConfig.set("settings_update_policy", args);
  //     // reset check date for latest version
  //     appConfig.delete(CheckForUpdatesLocalStorageName);

  //     event.reply("settings_update_policy", args);
  //     return;
  //   }

  //   event.reply("settings_update_policy", currentSettings);
  // });

  ipcMain.on(UpdatePolicyIpcKey, async (event, args) => {
    if (args === null || args === undefined) {
      if (appConfig.has(UpdatePolicySettings)) {
        const updatePolicy = (await appConfig.get(
          UpdatePolicySettings
        )) as boolean;
        if (updatePolicy !== null || updatePolicy !== undefined) {
          event.reply(UpdatePolicyIpcKey, updatePolicy);
          return;
        }
      }
      event.reply(UpdatePolicyIpcKey, true);
      return;
    }

    console.log("set arg --> ", args);
    appConfig.set(UpdatePolicySettings, args);
    // reset check date for latest version
    // appConfig.delete(CheckForUpdatesLocalStorageName);

    event.reply(UpdatePolicyIpcKey, args);
  });
}

export default ipcBridge;
