import { globalHistory } from "@reach/router";
import { render } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import MenuSearch from "./menu-search";

describe("MenuSearch", () => {
  it("renders", () => {
    render(<MenuSearch state={undefined as any} dispatch={jest.fn()} />);
  });

  describe("with Context", () => {
    it("open hamburger menu", () => {
      var component = render(
        <MenuSearch
          state={{ fileIndexItems: [] } as any}
          dispatch={jest.fn()}
        />
      );
      var hamburger = component.find(".hamburger");

      expect(component.exists(".form-nav")).toBeTruthy();
      expect(component.exists(".hamburger.open")).toBeFalsy();
      expect(component.exists(".nav.open")).toBeFalsy();

      act(() => {
        hamburger.simulate("click");
      });

      // find does not work
      expect(component.html()).toContain("hamburger open");
      expect(component.html()).toContain("nav open");
      expect(component.exists(".form-nav")).toBeTruthy();
      component.unmount();
    });

    it("un select items", () => {
      globalHistory.navigate("/?select=1");
      var component = render(
        <MenuSearch
          state={{ fileIndexItems: [] } as any}
          dispatch={jest.fn()}
        />
      );

      expect(globalHistory.location.search).toBe("?select=1");

      act(() => {
        component.find('[data-test="selected-1"]').simulate("click");
      });

      expect(globalHistory.location.search).toBe("");

      component.unmount();
      globalHistory.navigate("/");
    });
  });
});
