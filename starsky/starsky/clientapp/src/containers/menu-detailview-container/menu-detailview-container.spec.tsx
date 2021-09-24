import React from "react";
import * as MenuDetailView from "../../components/organisms/menu-detail-view/menu-detail-view";
import MenuDetailViewContainer from "./menu-detailview-container";

describe("MenuDetailViewContainer", () => {
  it("renders", () => {
    render(<MenuDetailViewContainer />);
  });

  it("expect child object", () => {
    const menuDetailViewSpy = jest
      .spyOn(MenuDetailView, "default")
      .mockImplementationOnce(() => null);
    var component = render(<MenuDetailViewContainer />);
    expect(menuDetailViewSpy).toBeCalled();
    component.unmount();
  });

  it("use context is null", () => {
    jest
      .spyOn(React, "useContext")
      .mockImplementationOnce(() => [null, jest.fn()]);
    const menuDetailViewSpy = jest
      .spyOn(MenuDetailView, "default")
      .mockImplementationOnce(() => null);

    var component = render(<MenuDetailViewContainer />);
    expect(menuDetailViewSpy).toBeCalled();

    component.unmount();
  });
});
