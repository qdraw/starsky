/* eslint-disable @typescript-eslint/no-unnecessary-type-assertion */
import { UpdatePolicyIpcKey } from "../../app/config/update-policy-ipc-key.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import {
  switchUpdatePolicyOffId,
  switchUpdatePolicyOnId
} from "./settings.const";

declare global {
  // eslint-disable-next-line vars-on-top, no-var
  var api: IPreloadApi;
}

export function settingsCheckForUpdatesToggle() {
  document
    .querySelector(switchUpdatePolicyOnId)
    .addEventListener("change", () => {
      window.api.send(UpdatePolicyIpcKey, true);
    });

  document
    .querySelector(switchUpdatePolicyOffId)
    .addEventListener("change", () => {
      window.api.send(UpdatePolicyIpcKey, false);
    });

  window.api.receive(UpdatePolicyIpcKey, (updateResult: boolean) => {
    const switchUpdatePolicyOff = document.querySelector(
      switchUpdatePolicyOffId
    ) as HTMLInputElement;
    switchUpdatePolicyOff.checked = !updateResult;
    switchUpdatePolicyOff.disabled = false;

    const switchUpdatePolicyOn = document.querySelector(
      switchUpdatePolicyOnId
    ) as HTMLInputElement;
    switchUpdatePolicyOn.checked = updateResult;
    switchUpdatePolicyOn.disabled = false;
  });

  window.api.send(UpdatePolicyIpcKey, null);
}
