import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "../../app/config/location-ipc-keys.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import {
  remoteLocationId,
  switchLocalId,
  switchRemoteId
} from "./settings.const";

declare global {
  var api: IPreloadApi;
}

export function settingsRemoteLocalToggle() {
  /** Switch remote local */
  document.querySelector(switchLocalId).addEventListener("change", function () {
    changeRemoteToggle(false);
  });

  document
    .querySelector(switchRemoteId)
    .addEventListener("change", function () {
      changeRemoteToggle(true);
    });

  window.api.send(LocationIsRemoteIpcKey, null);

  function changeRemoteToggle(isRemote: boolean) {
    window.api.send(LocationIsRemoteIpcKey, isRemote);
  }

  window.api.receive(LocationIsRemoteIpcKey, (isRemote: boolean) => {
    const switchLocal = document.querySelector(
      switchLocalId
    ) as HTMLInputElement;
    const switchRemote = document.querySelector(
      switchRemoteId
    ) as HTMLInputElement;
    const remoteLocation = document.querySelector(
      remoteLocationId
    ) as HTMLInputElement;

    if (isRemote) {
      switchLocal.checked = false;
      switchRemote.checked = true;
      remoteLocation.disabled = false;
      // trigger again to show results
      window.api.send(LocationUrlIpcKey, null);
    } else {
      switchLocal.checked = true;
      switchRemote.checked = false;
      remoteLocation.disabled = true;
    }
    switchLocal.disabled = false;
    switchRemote.disabled = false;
  });
}
