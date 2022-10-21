import { UpdatePolicyIpcKey } from "../../app/config/update-policy-ipc-key.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import {
  switchUpdatePolicyOffId,
  switchUpdatePolicyOnId
} from "./settings.const";

declare global {
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
    );
    switchUpdatePolicyOff.checked = !updateResult;
    switchUpdatePolicyOff.disabled = false;

    const switchUpdatePolicyOn = document.querySelector(
      switchUpdatePolicyOnId
    );
    switchUpdatePolicyOn.checked = updateResult;
    switchUpdatePolicyOn.disabled = false;
  });

  window.api.send(UpdatePolicyIpcKey, null);
}
