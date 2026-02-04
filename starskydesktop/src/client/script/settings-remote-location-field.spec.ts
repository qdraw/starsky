/* eslint-disable no-var */
/* eslint-disable @typescript-eslint/no-unnecessary-type-assertion */
/* eslint-disable @typescript-eslint/unbound-method */
/* eslint-disable vars-on-top */
import { LocationUrlIpcKey } from "../../app/config/location-ipc-keys.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import { settingsRemoteLocationField } from "./settings-remote-location-field";
import { locationIsValidId, remoteLocationId } from "./settings.const";

declare global {
  var api: IPreloadApi;
}
jest.mock("electron-settings", () => {
  return {
    get: () => "data",
    __esModule: true,
  };
});

describe("settings Remote Local Toggle", () => {
  afterEach(() => {
    document.body.innerHTML = "";
  });

  // eslint-disable-next-line jest/expect-expect
  it("render component", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    document.body.innerHTML = `<input id="${remoteLocationId.replace(
      "#",
      ""
    )}">`;

    settingsRemoteLocationField();
  });

  it("enter test123 value", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    const remoteLocationTag = remoteLocationId.replace("#", "");
    document.body.innerHTML = `<input id="${remoteLocationTag}" value="test123">`;

    // trigger to on
    const event = new Event("change");
    document.querySelector(remoteLocationId).dispatchEvent(event);

    // todo complete
    settingsRemoteLocationField();

    const event1 = new Event("change");
    document.querySelector(remoteLocationId).dispatchEvent(event1);

    expect(window.api.send).toHaveBeenCalled();
    expect(window.api.send).toHaveBeenNthCalledWith(1, LocationUrlIpcKey, null);
    expect(window.api.send).toHaveBeenNthCalledWith(
      2,
      LocationUrlIpcKey,
      "test123"
    );
  });

  it("enter emthy value", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    const remoteLocationTag = remoteLocationId.replace("#", "");
    document.body.innerHTML = `<input id="${remoteLocationTag}" value="">`;

    // trigger to on
    const event = new Event("change");
    document.querySelector(remoteLocationId).dispatchEvent(event);

    console.log("-should give console.error");
    settingsRemoteLocationField();

    const event1 = new Event("change");
    document.querySelector(remoteLocationId).dispatchEvent(event1);

    expect(window.api.send).toHaveBeenCalled();
    expect(window.api.send).toHaveBeenNthCalledWith(1, LocationUrlIpcKey, null);
  });

  it("receive from remote", () => {
    window.api = {
      send: jest.fn(),
      receive: (_, func) => {
        func({
          isLocal: false,
          location: "test"
        });
      }
    };
    const remoteLocationIdTag = remoteLocationId.replace("#", "");
    document.body.innerHTML = `<input id="${remoteLocationIdTag}" value="test123">`
      + `<div id="${locationIsValidId.replace("#", "")}"></div>`;

    settingsRemoteLocationField();

    // should update from window/api
    const remoteLocationValue = (document.querySelector(
      remoteLocationId
    ) as HTMLInputElement).value;

    expect(remoteLocationValue).toBe("test");
  });

  it("receive from remote with local param", () => {
    window.api = {
      send: jest.fn(),
      receive: (_, func) => {
        func({
          isLocal: true,
          location: "test"
        });
      }
    };
    const remoteLocationIdTag = remoteLocationId.replace("#", "");
    document.body.innerHTML = `<input id="${remoteLocationIdTag}" value="test123">`
      + `<div id="${locationIsValidId.replace("#", "")}"></div>`;

    settingsRemoteLocationField();

    // should ignore window/api
    const remoteLocationValue = (document.querySelector(
      remoteLocationId
    ) as HTMLInputElement).value;

    expect(remoteLocationValue).toBe("test123");
  });
});
