import { mount, shallow } from "enzyme";
import React from 'react';
import PreferencesAppSettings from './preferences-app-settings';

describe("PreferencesAppSettings", () => {

  it("renders", () => {
    shallow(<PreferencesAppSettings />)
  });

  describe("context", () => {

    it("default nothing entered", () => {
      var component = mount(<PreferencesAppSettings />);

      component.find('form [type="submit"]').first().simulate('submit');

      expect(component.find('.warning-box').text()).toBe("Enter the current and new password");

      component.unmount();
    });

  });
});