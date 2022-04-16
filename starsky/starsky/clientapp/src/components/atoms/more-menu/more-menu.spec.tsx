import { act, render } from "@testing-library/react";
import React from "react";
import MoreMenu, { MoreMenuEventCloseConst } from "./more-menu";

describe("More Menu", () => {
  it("renders", () => {
    render(<MoreMenu />);
  });

  it("get childeren", () => {
    const element = render(<MoreMenu>test</MoreMenu>);
    const menuOptions = element.queryAllByTestId("menu-options")[0];

    expect(menuOptions.innerHTML).toBe("test");
  });

  it("toggle", async () => {
    const element = render(<MoreMenu>test</MoreMenu>);

    const menuContext = element.queryAllByTestId("menu-context")[0];
    // need to await here
    await menuContext.click();

    expect(menuContext.className).toBe("menu-context");
  });

  it("toggle no childeren", () => {
    const element = render(<MoreMenu></MoreMenu>);

    const menuContext = element.queryAllByTestId("menu-context")[0];
    menuContext.click();

    expect(menuContext.className).toBe("menu-context menu-context--hide");
  });

  it("turn off using event", (done) => {
    var element = render(<MoreMenu>test</MoreMenu>);

    const menuContext = element.queryAllByTestId("menu-context")[0];

    window.addEventListener(MoreMenuEventCloseConst, () => {
      expect(menuContext.className).toBe("menu-context menu-context--hide");
      done();
    });

    act(() => {
      window.dispatchEvent(new CustomEvent(MoreMenuEventCloseConst));
    });
  });
});
