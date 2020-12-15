import { DefaultImageApplicationIpcKey } from "../../app/config/default-image-application-settings-ipc-key.const";
import { settingsDefaultImageApplicationSelect } from "./settings-default-image-application-select";
import {
  defaultImageApplicationFileSelector,
  defaultImageApplicationReset,
  defaultImageApplicationResult
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

  it("trigger open element", () => {
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

    // trigger to on
    var event = new Event("click");
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
      { showOpenDialog: true }
    );
  });

  it("trigger reset", () => {
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

    // trigger to on
    var event = new Event("click");
    document.querySelector(defaultImageApplicationReset).dispatchEvent(event);

    expect(window.api.send).toBeCalled();
    expect(window.api.send).toHaveBeenNthCalledWith(
      1,
      DefaultImageApplicationIpcKey,
      null
    );

    expect(window.api.send).toHaveBeenNthCalledWith(
      2,
      DefaultImageApplicationIpcKey,
      { reset: true }
    );
  });

  it("receive defaultImageApplicationResult", () => {
    window.api = {
      send: jest.fn(),
      receive: (_, func) => {
        func("data_received");
      }
    };

    document.body.innerHTML = `<div id='${defaultImageApplicationResult.replace(
      "#",
      ""
    )}'></div><div id='${defaultImageApplicationFileSelector.replace(
      "#",
      ""
    )}'></div><div id='${defaultImageApplicationReset.replace(
      "#",
      ""
    )}'></div>`;

    settingsDefaultImageApplicationSelect();

    const content = (document.querySelector(
      defaultImageApplicationResult
    ) as HTMLInputElement).innerHTML;

    expect(content).toBe("data_received");
  });
});
