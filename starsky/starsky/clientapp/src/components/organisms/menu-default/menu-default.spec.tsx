import { mount, shallow } from "enzyme";
import React from "react";
import HamburgerMenuToggle from "../../atoms/hamburger-menu-toggle/hamburger-menu-toggle";
import MenuDefault from "./menu-default";

describe("MenuDefault", () => {
  it("renders", () => {
    shallow(<MenuDefault isEnabled={false} />);
  });

  describe("with Context", () => {
    it("has hamburger", () => {
      var component = shallow(<MenuDefault isEnabled={true} />);
      expect(component.exists(HamburgerMenuToggle)).toBeTruthy();
    });

    it("check if on click the hamburger opens", () => {
      var component = mount(<MenuDefault isEnabled={true}>t</MenuDefault>);

      expect(component.exists('[data-test="hamburger"] .open')).toBeFalsy();

      component.find('[data-test="hamburger"]').simulate("click");
      expect(component.exists('[data-test="hamburger"] .open')).toBeTruthy();

      component.unmount();
    });
  });
});
