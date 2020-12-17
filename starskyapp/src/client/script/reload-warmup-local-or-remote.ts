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

  // https://stackoverflow.com/a/31431911
  const regex = new RegExp("^[^/]+/[^/].*$|^/[^/].*$", "gmi");
  if (rememberUrl && rememberUrl.match(regex)) {
    console.log(rememberUrl.match(regex)[0]);
    appendAfterDomainUrl = decodeURI(rememberUrl.match(regex)[0]);
  }
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
        document.title += ` going to ${locationData?.location}`;
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
