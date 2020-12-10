
// Expose protected methods that allow the renderer process to use

import { contextBridge, ipcRenderer } from "electron";
import { LocationIsRemoteIpcKey, LocationUrlIpcKey } from "../app/config/location-settings-ipc-keys.const";

// the ipcRenderer without exposing the entire object
contextBridge.exposeInMainWorld(
    "api", {
        send: (channel: string, data: any) => {
            // whitelist channels
            let validChannels = [LocationIsRemoteIpcKey, LocationUrlIpcKey];
            if (validChannels.includes(channel)) {
                ipcRenderer.send(channel, data);
            }
        },
        receive: (channel: string, func: Function) => {
            let validChannels = [LocationIsRemoteIpcKey, LocationUrlIpcKey];
            if (validChannels.includes(channel)) {
                // Deliberately strip event as it includes `sender` 
                ipcRenderer.on(channel, (event, ...args) => func(...args));
            }
        }
    }
);