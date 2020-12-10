
// Expose protected methods that allow the renderer process to use

import { contextBridge, ipcRenderer } from "electron";
import { AppVersionIpcKey } from "../app/config/app-version-ipc-key.const";
import { LocationIsRemoteIpcKey, LocationUrlIpcKey } from "../app/config/location-settings-ipc-keys.const";

// the ipcRenderer without exposing the entire object
contextBridge.exposeInMainWorld(
    "api", {
        send: (channel: string, data: any) => {
            // whitelist channels
            let validChannels = [LocationIsRemoteIpcKey, LocationUrlIpcKey, AppVersionIpcKey];
            if (validChannels.includes(channel)) {
                ipcRenderer.send(channel, data);
            }
        },
        receive: (channel: string, func: Function) => {
            let validChannels = [LocationIsRemoteIpcKey, LocationUrlIpcKey, AppVersionIpcKey];
            if (validChannels.includes(channel)) {
                // Deliberately strip event as it includes `sender` 
                ipcRenderer.on(channel, (event, ...args) => func(...args));
            }
        }
    }
);