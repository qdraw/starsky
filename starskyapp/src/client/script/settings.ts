import { IPreloadApi } from "../../preload/IPreloadApi";
import { settingsCheckForUpdatesToggle } from "./settings-check-for-updates-toggle";
import { settingsRemoteLocalToggle } from "./settings-remote-local-toggle";
import { settingsRemoteLocationField } from "./settings-remote-location-field";

declare global {
  var api: IPreloadApi;
}

settingsRemoteLocalToggle();

/** Web location field */
settingsRemoteLocationField();

/** Default app field */

/** Check for updates */
settingsCheckForUpdatesToggle();

// document.querySelector("#file_selector").addEventListener('click', function() {
//     window.api.send("settings_default_app", {
//         showOpenDialog: true
//     });
// });

// document.querySelector("#file_selector_reset").addEventListener('click', function() {
//     window.api.send("settings_default_app", {
//         reset: true
//     });
// });

// window.api.receive("settings_default_app", (data: string) => {
//     document.querySelector("#file_selector_result").innerHTML = data;
// });

// window.api.send("settings_default_app",null);
