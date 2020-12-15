import * as settingsCheckForUpdatesToggle from "./settings-check-for-updates-toggle";
import * as settingsDefaultImageApplicationSelect from "./settings-default-image-application-select";
import * as settingsRemoteLocalToggle from "./settings-remote-local-toggle";
import * as settingsRemoteLocationField from "./settings-remote-location-field";

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
    require("./settings");

    expect(checkSpy).toBeCalled();
    expect(checkRemoteToggleSpy).toBeCalled();
    expect(checkRemoteFieldSpy).toBeCalled();
    expect(settingsDefaultImageApplicationSelectSpy).toBeCalled();
  });
});
