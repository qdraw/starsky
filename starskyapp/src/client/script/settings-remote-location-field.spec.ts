import { LocationUrlIpcKey } from "../../app/config/location-ipc-keys.const";
import { IPreloadApi } from "../../preload/IPreloadApi";
import { settingsRemoteLocationField } from "./settings-remote-location-field";
import { locationIsValidId, remoteLocationId } from "./settings.const";

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
    var event = new Event("change");
    document.querySelector(remoteLocationId).dispatchEvent(event);

    // todo complete
    settingsRemoteLocationField();

    var event = new Event("change");
    document.querySelector(remoteLocationId).dispatchEvent(event);

    expect(window.api.send).toBeCalled();
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
    var event = new Event("change");
    document.querySelector(remoteLocationId).dispatchEvent(event);

    console.log("-should give console.error");
    settingsRemoteLocationField();

    var event = new Event("change");
    document.querySelector(remoteLocationId).dispatchEvent(event);

    expect(window.api.send).toBeCalled();
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
    document.body.innerHTML =
      `<input id="${remoteLocationIdTag}" value="test123">` +
      `<div id="${locationIsValidId.replace("#", "")}"></div>`;

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
    document.body.innerHTML =
      `<input id="${remoteLocationIdTag}" value="test123">` +
      `<div id="${locationIsValidId.replace("#", "")}"></div>`;

    settingsRemoteLocationField();

    // should ignore window/api
    const remoteLocationValue = (document.querySelector(
      remoteLocationId
    ) as HTMLInputElement).value;

    expect(remoteLocationValue).toBe("test123");
  });
});
