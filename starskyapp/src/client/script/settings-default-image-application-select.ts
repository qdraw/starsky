import {
  DefaultImageApplicationIpcKey,
  IDefaultImageApplicationProps
} from "../../app/config/default-image-application-settings-ipc-key.const";
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
      } as IDefaultImageApplicationProps);
    });

  document
    .querySelector(defaultImageApplicationReset)
    .addEventListener("click", function () {
      window.api.send(DefaultImageApplicationIpcKey, {
        reset: true
      } as IDefaultImageApplicationProps);
    });

  window.api.receive(DefaultImageApplicationIpcKey, (data: string | false) => {
    if (data) {
      document.querySelector(defaultImageApplicationResult).innerHTML = data;
    }
    if (data === false || data === undefined) {
      document.querySelector(defaultImageApplicationResult).innerHTML =
        "Defined by system";
    }
  });

  window.api.send(DefaultImageApplicationIpcKey, null);
}
