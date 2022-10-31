/* eslint-disable @typescript-eslint/no-unnecessary-type-assertion */
import { IlocationUrlSettings } from "../../app/config/IlocationUrlSettings";
import { LocationUrlIpcKey } from "../../app/config/location-ipc-keys.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import { locationIsValidId, remoteLocationId } from "./settings.const";

declare global {
  // eslint-disable-next-line vars-on-top, no-var
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
    // eslint-disable-next-line func-names
    .addEventListener("change", function () {
      // eslint-disable-next-line @typescript-eslint/no-unsafe-argument, @typescript-eslint/no-unsafe-member-access
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
