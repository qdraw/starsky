import { DefaultImageApplicationIpcKey } from "../../app/config/default-image-application-settings-ipc-key.const";
import { settingsDefaultImageApplicationSelect } from "./settings-default-image-application-select";
import {
  defaultImageApplicationFileSelector,
  defaultImageApplicationReset
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
    document.body.innerHTML = `<div id='${defaultImageApplicationFileSelector.replace(
      "#",
      ""
    )}'></div><div id='${defaultImageApplicationReset.replace(
      "#",
      ""
    )}'></div>`;

    settingsDefaultImageApplicationSelect();
  });

  it("change to on", () => {
    window.api = {
      send: jest.fn(),
      receive: jest.fn()
    };
    const fileSelectorId = defaultImageApplicationFileSelector.replace("#", "");
    document.body.innerHTML = `<div id='${defaultImageApplicationFileSelector.replace(
      "#",
      ""
    )}'></div><div id='${defaultImageApplicationReset.replace(
      "#",
      ""
    )}'></div>`;

    settingsDefaultImageApplicationSelect();

    // trigger to on
    var event = new Event("change");
    document
      .querySelector(defaultImageApplicationFileSelector)
      .dispatchEvent(event);

    expect(window.api.send).toBeCalled();
    expect(window.api.send).toHaveBeenNthCalledWith(
      1,
      DefaultImageApplicationIpcKey,
      null
    );
    expect(window.api.send).toHaveBeenNthCalledWith(
      2,
      DefaultImageApplicationIpcKey,
      true
    );
  });
});
