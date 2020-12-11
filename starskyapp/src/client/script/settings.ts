import { IlocationUrlSettings } from "../../app/config/IlocationUrlSettings";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "../../app/config/location-ipc-keys.const";
import { UpdatePolicyIpcKey } from "../../app/config/update-policy-ipc-key.const";
import { IPreloadApi } from "../../preload/IPreloadApi";

declare global {
  var api: IPreloadApi;
}

/** Switch remote local */
document.querySelector("#switch_local").addEventListener("change", function () {
  changeRemoteToggle(false);
});

document
  .querySelector("#switch_remote")
  .addEventListener("change", function () {
    changeRemoteToggle(true);
  });

window.api.send(LocationIsRemoteIpcKey, null);

function changeRemoteToggle(isRemote: boolean) {
  window.api.send(LocationIsRemoteIpcKey, isRemote);
}

window.api.receive(LocationIsRemoteIpcKey, (isRemote: boolean) => {
  const switchLocal = document.querySelector(
    "#switch_local"
  ) as HTMLInputElement;
  const switchRemote = document.querySelector(
    "#switch_remote"
  ) as HTMLInputElement;
  const remoteLocation = document.querySelector(
    "#remote_location"
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
});

/** Web location field */

function changeRemoteLocation(location: string) {
  if (!location) {
    console.error("wrong location");
    return;
  }
  window.api.send(LocationUrlIpcKey, location);
}

document
  .querySelector("#remote_location")
  .addEventListener("change", function () {
    changeRemoteLocation(this.value);
  });

window.api.send(LocationUrlIpcKey, null);

window.api.receive(LocationUrlIpcKey, (result: IlocationUrlSettings) => {
  const remoteLocation = document.querySelector(
    "#remote_location"
  ) as HTMLInputElement;

  if (!result.isLocal) {
    remoteLocation.value = result.location;
  }

  if (result && result.isValid !== null) {
    document.querySelector("#locationOk").innerHTML = result.isValid
      ? "Setting is saved"
      : "FAIL setting is not valid and NOT saved";
  }
});

/** Default app field */

/** Check for updates */

document
  .querySelector("#switch_update_policy_on")
  .addEventListener("change", function () {
    window.api.send(UpdatePolicyIpcKey, true);
  });

window.api.receive(UpdatePolicyIpcKey, (data: boolean) => {
  if (!data) {
    const switchUpdatePolicyOff = document.querySelector(
      "#switch_update_policy_off"
    ) as HTMLInputElement;
    switchUpdatePolicyOff.checked = true;

    const switchUpdatePolicyOn = document.querySelector(
      "#switch_update_policy_on"
    ) as HTMLInputElement;
    switchUpdatePolicyOn.checked = null;
  }
});

window.api.send("settings_update_policy", null);

// document.querySelector("#file_selector").addEventListener('click', function() {
//     window.api.send("settings_default_app", {
//         showOpenDialog: true
//     });
// });

// document.querySelector("#file_selector_reset").addEventListener('click', function() {
//     window.api.send("settings_default_app", {
//         reset: true
//     });
// });

// window.api.receive("settings_default_app", (data: string) => {
//     document.querySelector("#file_selector_result").innerHTML = data;
// });

// window.api.send("settings_default_app",null);

// document.querySelector('#switch_update_policy_off').addEventListener('change', function() {
//     window.api.send("settings_update_policy",false);

// });

// document.querySelector('#switch_update_policy_on').addEventListener('change', function() {
//     window.api.send("settings_update_policy",true);
// });

// window.api.receive("settings_update_policy", (data) => {
//     if (!data) {
//         document.querySelector("#switch_update_policy_off").checked = true;
//         document.querySelector("#switch_update_policy_on").checked = null;
//     }
// });

// window.api.send("settings_update_policy",null);
