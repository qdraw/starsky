import { AppVersionIpcKey } from "../../app/config/app-version-ipc-key.const";
import { IlocationUrlSettings } from "../../app/config/IlocationUrlSettings";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "../../app/config/location-settings-ipc-keys.const";
import UrlQuery from "../../app/config/url-query";
import { IPreloadApi } from "../../preload/IPreloadApi";

declare global {
  var api: IPreloadApi;
}

/**
 * no slash at end
 * @param {*} domainUrl
 * @param {*} count
 * @param {*} maxCount
 */
function warmupScript(
  domainUrl: string,
  count: number,
  maxCount: number,
  callback: Function
): void {
  fetch(domainUrl + new UrlQuery().HealthApi())
    .then((response) => {
      if (response.status === 200 || response.status === 503) {
        response.text().then((text) => {
          callback(text.includes(new UrlQuery().HealthShouldContain()));
        });
        return;
      }
      next();
    })
    .catch((error) => {
      console.log("error", error);
      next();
    });

  function next() {
    if (count <= maxCount) {
      count++;
      setTimeout(() => {
        warmupScript(domainUrl, count, maxCount, callback);
      }, 200);
    } else {
      console.log(
        "no connection to the internal component, please restart the application"
      );
      callback(false);
    }
  }
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
  window.location.href = domainUrl + appendAfterDomainUrl;
}

function checkForUpdates(domainUrl: string, apiVersion: string): Promise<void> {
  return new Promise(function (resolve, reject) {
    fetch(domainUrl + new UrlQuery().HealthVersionApi(), {
      method: "POST",
      headers: {
        "x-api-version": `${apiVersion}`
      }
    })
      .then((versionResponse) => {
        console.log(versionResponse);

        if (versionResponse.status === 200) {
          resolve();
          return;
        }

        if (
          versionResponse.status === 400 &&
          document.querySelectorAll(".upgrade").length === 1
        ) {
          const upgradeElement = document.querySelector(
            ".upgrade"
          ) as HTMLElement;
          if (upgradeElement) {
            upgradeElement.style.display = "block";
          }

          const preloaderElement = document.querySelector(
            ".preloader"
          ) as HTMLElement;
          if (preloaderElement) {
            preloaderElement.style.display = "none";
          }
          return;
        }

        alert(
          `#${versionResponse.status} - Version check failed, please try to restart the application`
        );
        reject();
      })
      .catch((error) => {
        alert("no connection to version check, please restart the application");
        reject();
      });
  });
}

function warmupLocalOrRemote() {
  window.api.send(LocationIsRemoteIpcKey, null);

  // window.api.receive(LocationIsRemoteIpcKey, (isRemote : any) => {

  window.api.send(AppVersionIpcKey, null);

  window.api.receive(AppVersionIpcKey, (appVersion: any) => {
    // if (isRemote == false) {
    //   document.title += ` going to default`
    //   const defaultDomain = 'http://localhost:9609';

    //   warmupScript(defaultDomain, 0, 300,()=>{
    //     checkForUpdates(defaultDomain, appVersion)
    //       .then(()=> redirecter(defaultDomain))
    //       .catch(()=>{});
    //   })
    //   return;
    // }

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
  // });
}

warmupLocalOrRemote();
