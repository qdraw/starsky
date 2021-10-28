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

      let hamburger = component.queryByTestId("hamburger") as HTMLDivElement;
      let hamburgerDiv = hamburger.querySelector("div") as HTMLDivElement;
      expect(hamburgerDiv.className).toBe("hamburger");

      act(() => {
        hamburger.click();
      });

      hamburger = component.queryByTestId("hamburger") as HTMLDivElement;
      hamburgerDiv = hamburger.querySelector("div") as HTMLDivElement;
      expect(hamburgerDiv.className).toBe("hamburger open");

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

      let selected1 = component.queryByTestId("selected-1") as HTMLDivElement;

      act(() => {
        selected1.click();
      });

      expect(globalHistory.location.search).toBe("");

      component.unmount();
      globalHistory.navigate("/");
    });
  });
});
