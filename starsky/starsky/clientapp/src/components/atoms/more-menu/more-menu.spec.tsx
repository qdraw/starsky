import { act, render } from "@testing-library/react";
import React from "react";
import MoreMenu, { MoreMenuEventCloseConst } from "./more-menu";

describe("More Menu", () => {
  it("renders", () => {
    render(<MoreMenu />);
  });

  it("get childeren", () => {
    var element = render(<MoreMenu>test</MoreMenu>);
    expect(element.find(".menu-options").text()).toBe("test");
  });

  it("toggle", () => {
    var element = render(<MoreMenu>test</MoreMenu>);

    act(() => {
      element.find(".menu-context").simulate("click");
    });

    expect(element.find(".menu-context").props().className).toBe(
      "menu-context"
    );
  });

  it("toggle no childeren", () => {
    var element = render(<MoreMenu />);

    act(() => {
      element.find(".menu-context").simulate("click");
    });

    expect(element.find(".menu-context").props().className).toBe(
      "menu-context menu-context--hide"
    );
  });

  it("turn off using event", (done) => {
    var element = render(<MoreMenu>test</MoreMenu>);

    act(() => {
      element.find(".menu-context").simulate("click");
    });

    window.addEventListener(MoreMenuEventCloseConst, () => {
      expect(element.find(".menu-context").props().className).toBe(
        "menu-context menu-context--hide"
      );
      done();
    });

    act(() => {
      window.dispatchEvent(new CustomEvent(MoreMenuEventCloseConst));
    });
  });
});
