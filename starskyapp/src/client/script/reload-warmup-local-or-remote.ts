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
          const notFoundWarning = document.querySelectorAll(".not-found-warning");
          if (notFoundWarning && notFoundWarning[0]) {
            const preloaderIcon = document.querySelectorAll(".preloader--icon");
            if (preloaderIcon && preloaderIcon[0]) {
              preloaderIcon[0].classList.add("hide");
            }
            
            notFoundWarning[0].classList.add("show");
            notFoundWarning[0].innerHTML = "There was an error loading " + locationData.location;
            if (!locationData.isLocal) {
              notFoundWarning[0].innerHTML += "<br />This is a remote service"
            }
            else {
              notFoundWarning[0].innerHTML += "<br />This is a local service"
            }
          }
          else {
            alert("There was an error loading: "+ locationData.location + " isLocal:" + locationData.isLocal);
          }
        });
      }
    );
  });
}
