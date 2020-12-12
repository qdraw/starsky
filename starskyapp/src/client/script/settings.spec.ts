import * as settingsCheckForUpdatesToggle from "./settings-check-for-updates-toggle";
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

    // when change also update webpack and html
    require("./settings");

    expect(checkSpy).toBeCalled();
    expect(checkRemoteToggleSpy).toBeCalled();
    expect(checkRemoteFieldSpy).toBeCalled();
  });
});
