import { BrowserWindow } from "electron";

export function onHeaderReceived(newWindow: BrowserWindow) {
  newWindow.webContents.session.webRequest.onHeadersReceived(
    (res, callback) => {
      // @TODO: re-enable

      // var currentSettings = appConfig.get("remote_settings_" + isPackaged());
      // var localhost = "http://localhost:9609 "; // with space on end
      // ${appPort}

      // let whitelistDomain = localhost;
      // if (currentSettings && currentSettings.location) {
      //   whitelistDomain = !currentSettings.location ? localhost : localhost + new URL(currentSettings.location).origin;
      // }

      /// default-src 'none'; img-src 'self' https://a.tile.openstreetmap.org/ https://b.tile.openstreetmap.org/ https://c.tile.openstreetmap.org/; script-src 'self'; connect-src 'self' wss://starsky.server ;style-src 'self'; font-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'; object-src 'none'; media-src 'self'; frame-src 'none'; manifest-src 'self'; block-all-mixed-content;

      // // When change also check if CSPMiddleware needs to be updated
      // var csp = "default-src 'none'; img-src 'self' file://* https://www.openstreetmap.org https://tile.openstreetmap.org https://*.tile.openstreetmap.org "
      //  + whitelistDomain + "; " +      "style-src file://* unsafe-inline https://www.openstreetmap.org " + whitelistDomain
      //   + "; script-src 'self' https://js.monitor.azure.com/scripts/b/ai.2.min.js file://* https://az416426.vo.msecnd.net; " +
      //   "connect-src 'self' https://dc.services.visualstudio.com/v2/track https://*.in.applicationinsights.azure.com//v2/track " + whitelistDomain + "; " +
      //   "font-src file://* " + whitelistDomain + "; media-src " + whitelistDomain + ";";

      // if (!res.url.startsWith('devtools://') && !res.url.startsWith('http://localhost:3000/')  ) {
      //   res.responseHeaders["Content-Security-Policy"] = csp;
      // }

      callback({ cancel: false, responseHeaders: res.responseHeaders });
    }
  );
}
