import { act, render, screen } from "@testing-library/react";
import React from "react";
import MoreMenu, { MoreMenuEventCloseConst } from "./more-menu";

describe("More Menu", () => {
  it("renders", () => {
    render(<MoreMenuWrapper></MoreMenuWrapper>);
  });

  function MoreMenuWrapper() {
    const [_, setEnableMoreMenu] = React.useState(false);
    return <MoreMenu setEnableMoreMenu={setEnableMoreMenu}>test</MoreMenu>;
  }

  it("menu-menu-button should open", () => {
    render(<MoreMenuWrapper></MoreMenuWrapper>);

    const moreMenuButton = screen.getByTestId("menu-menu-button");
    moreMenuButton.click();

    expect(moreMenuButton.className).toBe("item item--more");
  });

  it("get childeren", () => {
    const element = render(<MoreMenuWrapper></MoreMenuWrapper>);
    const menuOptions = element.queryAllByTestId("menu-options")[0];

    expect(menuOptions.innerHTML).toBe("test");
  });

  it("toggle", async () => {
    const element = render(<MoreMenuWrapper></MoreMenuWrapper>);

    const menuContext = element.queryAllByTestId("menu-context")[0];
    // need to await here
    await menuContext.click();

    expect(menuContext.className).toBe("menu-context menu-context--hide");
  });

  it("toggle no childeren", () => {
    const element = render(<MoreMenuWrapper></MoreMenuWrapper>);

    const menuContext = element.queryAllByTestId("menu-context")[0];
    menuContext.click();

    expect(menuContext.className).toBe("menu-context menu-context--hide");
  });

  it("turn off using event", (done) => {
    const element = render(<MoreMenuWrapper></MoreMenuWrapper>);

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
