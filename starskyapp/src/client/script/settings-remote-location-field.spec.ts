import { IPreloadApi } from "../../preload/IPreloadApi";
import { settingsRemoteLocationField } from "./settings-remote-location-field";
import { remoteLocationId } from "./settings.const";

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

  xit("enter emthy value", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    const remoteLocationTag = remoteLocationId.replace("#", "");
    document.body.innerHTML = `<input id="${remoteLocationTag}">`;

    // trigger to on
    var event = new Event("change");
    document.querySelector(remoteLocationId).dispatchEvent(event);

    // todo complete
    settingsRemoteLocationField();
  });
});
