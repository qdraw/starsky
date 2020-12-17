import { UpdatePolicyIpcKey } from "../../app/config/update-policy-ipc-key.const";
import { settingsCheckForUpdatesToggle } from "./settings-check-for-updates-toggle";
import {
  switchUpdatePolicyOffId,
  switchUpdatePolicyOnId
} from "./settings.const";

describe("reload redirect", () => {
  afterEach(() => {
    document.body.innerHTML = "";
  });

  it("render component", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    document.body.innerHTML = `<div id='${switchUpdatePolicyOnId.replace(
      "#",
      ""
    )}'></div><div id='${switchUpdatePolicyOffId.replace("#", "")}'></div>`;

    settingsCheckForUpdatesToggle();
  });

  it("change to on", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    const switchUpdatePolicyOn = switchUpdatePolicyOnId.replace("#", "");
    document.body.innerHTML = `<div id='${switchUpdatePolicyOn}'></div><div id='${switchUpdatePolicyOffId.replace(
      "#",
      ""
    )}'></div>`;

    console.log(document.body.innerHTML);

    settingsCheckForUpdatesToggle();

    // trigger to on
    var event = new Event("change");
    document.querySelector(switchUpdatePolicyOnId).dispatchEvent(event);

    expect(window.api.send).toBeCalled();
    expect(window.api.send).toHaveBeenNthCalledWith(
      1,
      UpdatePolicyIpcKey,
      null
    );
    expect(window.api.send).toHaveBeenNthCalledWith(
      2,
      UpdatePolicyIpcKey,
      true
    );
  });

  it("change to off", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    const switchUpdatePolicyOn = switchUpdatePolicyOnId.replace("#", "");
    document.body.innerHTML = `<div id='${switchUpdatePolicyOn}'></div><div id='${switchUpdatePolicyOffId.replace(
      "#",
      ""
    )}'></div>`;

    settingsCheckForUpdatesToggle();

    // trigger to on
    var event = new Event("change");
    document.querySelector(switchUpdatePolicyOffId).dispatchEvent(event);

    expect(window.api.send).toBeCalled();
    expect(window.api.send).toHaveBeenNthCalledWith(
      1,
      UpdatePolicyIpcKey,
      null
    );
    expect(window.api.send).toHaveBeenNthCalledWith(
      2,
      UpdatePolicyIpcKey,
      false
    );
  });

  it("receive to on", () => {
    window.api = {
      send: jest.fn(),
      receive: (_, func) => {
        func(true);
      }
    };
    const switchUpdatePolicyOn = switchUpdatePolicyOnId.replace("#", "");
    document.body.innerHTML = `<input type="radio" id='${switchUpdatePolicyOn}' /><input type="radio" id='${switchUpdatePolicyOffId.replace(
      "#",
      ""
    )}' />`;

    settingsCheckForUpdatesToggle();

    const onToggleChecked = (document.querySelector(
      switchUpdatePolicyOnId
    ) as HTMLInputElement).checked;

    const offToggleChecked = (document.querySelector(
      switchUpdatePolicyOffId
    ) as HTMLInputElement).checked;

    expect(onToggleChecked).toBeTruthy();
    expect(offToggleChecked).toBeFalsy();
  });

  it("receive to off", () => {
    window.api = {
      send: jest.fn(),
      receive: (_, func) => {
        func(false);
      }
    };
    const switchUpdatePolicyOn = switchUpdatePolicyOnId.replace("#", "");
    document.body.innerHTML = `<input type="radio" id='${switchUpdatePolicyOn}' /><input type="radio" id='${switchUpdatePolicyOffId.replace(
      "#",
      ""
    )}' />`;

    settingsCheckForUpdatesToggle();

    const onToggleChecked = (document.querySelector(
      switchUpdatePolicyOnId
    ) as HTMLInputElement).checked;

    const offToggleChecked = (document.querySelector(
      switchUpdatePolicyOffId
    ) as HTMLInputElement).checked;

    expect(onToggleChecked).toBeFalsy();
    expect(offToggleChecked).toBeTruthy();
  });
});
