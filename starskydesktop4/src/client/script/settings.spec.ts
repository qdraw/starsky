import * as settingsCheckForUpdatesToggle from "./settings-check-for-updates-toggle";
import * as settingsDefaultImageApplicationSelect from "./settings-default-image-application-select";
import * as settingsRemoteLocalToggle from "./settings-remote-local-toggle";
import * as settingsRemoteLocationField from "./settings-remote-location-field";

jest.mock("electron-settings", () => {
  return {
    get: () => "data",
    __esModule: true,
  };
});

describe("settings", () => {
  it("should call deps", () => {
    const checkSpy = jest
      .spyOn(settingsCheckForUpdatesToggle, "settingsCheckForUpdatesToggle")
      .mockImplementationOnce(() => {});
    const checkRemoteToggleSpy = jest
      .spyOn(settingsRemoteLocalToggle, "settingsRemoteLocalToggle")
      .mockImplementationOnce(() => {});
    const checkRemoteFieldSpy = jest
      .spyOn(settingsRemoteLocationField, "settingsRemoteLocationField")
      .mockImplementationOnce(() => {});

    const settingsDefaultImageApplicationSelectSpy = jest
      .spyOn(
        settingsDefaultImageApplicationSelect,
        "settingsDefaultImageApplicationSelect"
      )
      .mockImplementationOnce(() => {});

    // when change also update webpack and html
    // eslint-disable-next-line global-require
    require("./settings");

    expect(checkSpy).toHaveBeenCalled();
    expect(checkRemoteToggleSpy).toHaveBeenCalled();
    expect(checkRemoteFieldSpy).toHaveBeenCalled();
    expect(settingsDefaultImageApplicationSelectSpy).toHaveBeenCalled();
  });
});
