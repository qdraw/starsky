import { render } from "@testing-library/react";
import React from "react";
import { MemoryRouter } from "react-router-dom";
import * as MenuDetailView from "../../components/organisms/menu-detail-view/menu-detail-view";
import MenuDetailViewContainer from "./menu-detailview-container";

describe("MenuDetailViewContainer", () => {
  it("renders", () => {
    render(
      <MemoryRouter>
        <MenuDetailViewContainer />
      </MemoryRouter>
    );
  });

  it("expect child object", () => {
    const menuDetailViewSpy = jest
      .spyOn(MenuDetailView, "default")
      .mockImplementationOnce(() => null);
    const component = render(
      <MemoryRouter>
        <MenuDetailViewContainer />
      </MemoryRouter>
    );
    expect(menuDetailViewSpy).toHaveBeenCalled();
    component.unmount();
  });

  it("use context is null", () => {
    jest.spyOn(React, "useContext").mockImplementationOnce(() => [null, jest.fn()]);
    const menuDetailViewSpy = jest
      .spyOn(MenuDetailView, "default")
      .mockImplementationOnce(() => null);

    const component = render(
      <MemoryRouter>
        <MenuDetailViewContainer />
      </MemoryRouter>
    );
    expect(menuDetailViewSpy).toHaveBeenCalled();

    component.unmount();
  });
});
