import { LocationIsRemoteIpcKey } from "../../app/config/location-ipc-keys.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import { settingsRemoteLocalToggle } from "./settings-remote-local-toggle";
import {
  remoteLocationId,
  switchLocalId,
  switchRemoteId
} from "./settings.const";
declare global {
  var api: IPreloadApi;
}

describe("settings Remote Local Toggle", () => {
  afterEach(() => {
    document.body.innerHTML = "";
  });

  it("render component", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    document.body.innerHTML = `<div id='${switchRemoteId.replace(
      "#",
      ""
    )}'></div><div id='${switchLocalId.replace("#", "")}'></div>`;

    settingsRemoteLocalToggle();
  });

  it("change to on", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    const switchRemoteIdTag = switchRemoteId.replace("#", "");
    document.body.innerHTML = `<div id='${switchRemoteIdTag}'></div><div id='${switchLocalId.replace(
      "#",
      ""
    )}'></div>`;

    settingsRemoteLocalToggle();

    // trigger to on
    var event = new Event("change");
    document.querySelector(switchRemoteId).dispatchEvent(event);

    expect(window.api.send).toBeCalled();
    expect(window.api.send).toHaveBeenNthCalledWith(
      1,
      LocationIsRemoteIpcKey,
      null
    );
    expect(window.api.send).toHaveBeenNthCalledWith(
      2,
      LocationIsRemoteIpcKey,
      true
    );
  });

  it("change to off", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    const switchRemoteIdTag = switchRemoteId.replace("#", "");
    document.body.innerHTML = `<div id='${switchRemoteIdTag}'></div><div id='${switchLocalId.replace(
      "#",
      ""
    )}'></div>`;

    settingsRemoteLocalToggle();

    // trigger to on
    var event = new Event("change");
    document.querySelector(switchLocalId).dispatchEvent(event);

    expect(window.api.send).toBeCalled();
    expect(window.api.send).toHaveBeenNthCalledWith(
      1,
      LocationIsRemoteIpcKey,
      null
    );
    expect(window.api.send).toHaveBeenNthCalledWith(
      2,
      LocationIsRemoteIpcKey,
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
    const switchRemoteIdTag = switchRemoteId.replace("#", "");
    document.body.innerHTML = `<div id="${remoteLocationId.replace(
      "#",
      ""
    )}"></div> <input type="radio" id='${switchRemoteIdTag}' /><input type="radio" id='${switchLocalId.replace(
      "#",
      ""
    )}' />`;

    settingsRemoteLocalToggle();

    const onToggleChecked = (document.querySelector(
      switchRemoteId
    ) as HTMLInputElement).checked;

    const offToggleChecked = (document.querySelector(
      switchLocalId
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
    const switchRemoteIdTag = switchRemoteId.replace("#", "");
    document.body.innerHTML = `<div id="${remoteLocationId.replace(
      "#",
      ""
    )}"></div> <input type="radio" id='${switchRemoteIdTag}' /><input type="radio" id='${switchLocalId.replace(
      "#",
      ""
    )}' />`;

    settingsRemoteLocalToggle();

    const onToggleChecked = (document.querySelector(
      switchRemoteId
    ) as HTMLInputElement).checked;

    const offToggleChecked = (document.querySelector(
      switchLocalId
    ) as HTMLInputElement).checked;

    expect(onToggleChecked).toBeFalsy();
    expect(offToggleChecked).toBeTruthy();
  });
});
