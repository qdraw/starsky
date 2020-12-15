import { DefaultImageApplicationIpcKey } from "../../app/config/default-image-application-settings-ipc-key.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import {
  defaultImageApplicationFileSelector,
  defaultImageApplicationReset,
  defaultImageApplicationResult
} from "./settings.const";

declare global {
  var api: IPreloadApi;
}

export function settingsDefaultImageApplicationSelect() {
  document
    .querySelector(defaultImageApplicationFileSelector)
    .addEventListener("click", function () {
      window.api.send(DefaultImageApplicationIpcKey, {
        showOpenDialog: true
      });
    });

  document
    .querySelector(defaultImageApplicationReset)
    .addEventListener("click", function () {
      window.api.send(DefaultImageApplicationIpcKey, {
        reset: true
      });
    });

  window.api.receive(DefaultImageApplicationIpcKey, (data: string) => {
    document.querySelector(defaultImageApplicationResult).innerHTML = data;
  });

  window.api.send(DefaultImageApplicationIpcKey, null);
}
