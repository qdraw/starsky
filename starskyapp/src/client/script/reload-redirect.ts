import { AppVersionIpcKey } from "../../app/config/app-version-ipc-key.const";
import { IlocationUrlSettings } from "../../app/config/IlocationUrlSettings";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "../../app/config/location-ipc-keys.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import { checkForUpdates } from "./check-for-updates";
import { warmupScript } from "./reload-warmup-script";

declare global {
  var api: IPreloadApi;
}

function redirecter(domainUrl: string) {
  var appendAfterDomainUrl = "";
  var rememberUrl = new URLSearchParams(window.location.search).get(
    "remember-url"
  );
  if (rememberUrl) {
    appendAfterDomainUrl = decodeURI(rememberUrl);
  }

  console.log(domainUrl + appendAfterDomainUrl);
  window.location.assign(domainUrl + appendAfterDomainUrl);
}

export function warmupLocalOrRemote() {
  window.api.send(LocationIsRemoteIpcKey, null);
  window.api.send(AppVersionIpcKey, null);

  window.api.receive(AppVersionIpcKey, (appVersion: any) => {
    window.api.send(LocationUrlIpcKey, null);

    window.api.receive(
      LocationUrlIpcKey,
      (locationData: IlocationUrlSettings) => {
        document.title += ` going to ${locationData.location}`;
        warmupScript(locationData.location, 0, 300, (isOk: boolean) => {
          if (isOk) {
            checkForUpdates(locationData.location, appVersion)
              .then(() => redirecter(locationData.location))
              .catch((e) => {
                console.log(e);
              });
            return;
          }
          alert("The domain in te configuration is not valid");
        });
      }
    );
  });
}

// detecting if running in browser vs node
if (typeof process === undefined) {
  warmupLocalOrRemote();
}
