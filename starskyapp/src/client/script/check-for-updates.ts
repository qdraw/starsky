import UrlQuery from "../../app/config/url-query";

export function checkForUpdates(
  domainUrl: string,
  apiVersion: string
): Promise<number> {
  return new Promise(function (resolve, reject) {
    fetch(domainUrl + new UrlQuery().HealthVersionApi(apiVersion), {
      method: "POST",
      headers: {
        "x-api-version": `${apiVersion}`
      }
    })
      .then((versionResponse) => {
        if (versionResponse.status === 200) {
          resolve(200);
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
          resolve(400);
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
