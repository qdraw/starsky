import { IPreloadApi } from "../../preload/IPreloadApi";
import { settingsCheckForUpdatesToggle } from "./settings-check-for-updates-toggle";
import { settingsDefaultImageApplicationSelect } from "./settings-default-image-application-select";
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

settingsDefaultImageApplicationSelect();
