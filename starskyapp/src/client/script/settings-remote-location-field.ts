import { IlocationUrlSettings } from "../../app/config/IlocationUrlSettings";
import { LocationUrlIpcKey } from "../../app/config/location-ipc-keys.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import { locationIsValidId, remoteLocationId } from "./settings.const";

declare global {
  var api: IPreloadApi;
}

export function settingsRemoteLocationField() {
  function changeRemoteLocation(location: string) {
    if (!location) {
      console.error("wrong location");
      return;
    }
    window.api.send(LocationUrlIpcKey, location);
  }

  document
    .querySelector(remoteLocationId)
    .addEventListener("change", function () {
      changeRemoteLocation(this.value);
    });

  window.api.send(LocationUrlIpcKey, null);

  window.api.receive(LocationUrlIpcKey, (result: IlocationUrlSettings) => {
    const remoteLocation = document.querySelector(
      remoteLocationId
    ) as HTMLInputElement;

    if (!result.isLocal) {
      remoteLocation.value = result.location;
    }

    if (result && result.isValid !== null) {
      document.querySelector(locationIsValidId).innerHTML = result.isValid
        ? "Setting is saved"
        : "FAIL setting is not valid and NOT saved";
    }
  });
}
