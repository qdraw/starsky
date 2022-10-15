import { render } from "@testing-library/react";
import React from "react";
import * as PreferencesAppSettings from "../components/organisms/preferences-app-settings/preferences-app-settings";
import * as PreferencesPassword from "../components/organisms/preferences-password/preferences-password";
import * as PreferencesUsername from "../components/organisms/preferences-username/preferences-username";
import { Preferences } from "./preferences";

describe("Preferences", () => {
  it("renders", () => {
    render(<Preferences />);
  });

  describe("status", () => {
    it("should mount child components", () => {
      const preferencesUsernameSpy = jest
        .spyOn(PreferencesUsername, "default")
        .mockImplementationOnce(() => <></>);
      const preferencesPasswordSpy = jest
        .spyOn(PreferencesPassword, "default")
        .mockImplementationOnce(() => <></>);
      const preferencesAppSettingsSpy = jest
        .spyOn(PreferencesAppSettings, "default")
        .mockImplementationOnce(() => <></>);

      const component = render(<Preferences />);

      expect(preferencesUsernameSpy).toBeCalled();
      expect(preferencesPasswordSpy).toBeCalled();
      expect(preferencesAppSettingsSpy).toBeCalled();

      component.unmount();
    });
  });
});
