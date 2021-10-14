import { render } from "@testing-library/react";
import React from "react";
import MenuDefault from "./menu-default";

describe("MenuDefault", () => {
  it("renders", () => {
    render(<MenuDefault isEnabled={false} />);
  });

  describe("with Context", () => {
    it("has hamburger", () => {
      var component = render(<MenuDefault isEnabled={true} />);
      expect(component.queryByTestId("hamburger")).toBeTruthy();
    });

    it("check if on click the hamburger opens", () => {
      var component = render(<MenuDefault isEnabled={true}>t</MenuDefault>);

      const hamburger = component.queryByTestId("hamburger");
      expect(hamburger?.querySelector(".open")).toBeFalsy();

      hamburger?.click();
      expect(hamburger?.querySelector(".open")).toBeTruthy();

      component.unmount();
    });
  });
});
