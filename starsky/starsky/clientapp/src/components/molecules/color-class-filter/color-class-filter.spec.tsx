import { act, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { Router } from "../../../router-app/router-app";
import { URLPath } from "../../../shared/url-path";
import ColorClassFilter from "./color-class-filter";
describe("ColorClassFilter", () => {
  it("renders", () => {
    render(
      <MemoryRouter>
        <ColorClassFilter
          itemsCount={1}
          subPath={"/test"}
          colorClassActiveList={[1, 2]}
          colorClassUsage={[1, 2]}
        ></ColorClassFilter>
      </MemoryRouter>
    );
  });

  it("onClick value", () => {
    const component = render(
      <MemoryRouter>
        <ColorClassFilter
          itemsCount={1}
          subPath={"/test"}
          colorClassActiveList={[1]}
          colorClassUsage={[1, 2]}
        />
      </MemoryRouter>
    );

    const colorClass = screen.queryByTestId(
      "color-class-filter-2"
    ) as HTMLAnchorElement;
    expect(colorClass).toBeTruthy();
    colorClass.click();

    component.unmount();
  });

  it("itemsCount = 0 should return nothing", () => {
    const component = render(
      <ColorClassFilter
        itemsCount={0}
        subPath={"/test"}
        colorClassActiveList={[1]}
        colorClassUsage={[1, 2]}
      />
    );

    expect(component).toBeTruthy();
    expect(component.container.innerHTML).toBeFalsy();

    component.unmount();
  });

  it("outside current scope display reset", () => {
    const component = render(
      <MemoryRouter>
        <ColorClassFilter
          itemsCount={1}
          subPath={"/test"}
          colorClassActiveList={[1]}
          colorClassUsage={[3]}
        />
      </MemoryRouter>
    );

    expect(screen.getByTestId("color-class-filter-reset")).toBeTruthy();

    component.unmount();
  });

  it("onClick value and preloader exist", () => {
    const component = render(
      <MemoryRouter>
        <ColorClassFilter
          itemsCount={1}
          subPath={"/test"}
          colorClassActiveList={[1]}
          colorClassUsage={[1, 2]}
        />
      </MemoryRouter>
    );

    const colorClass = screen.queryByTestId(
      "color-class-filter-2"
    ) as HTMLAnchorElement;
    expect(colorClass).toBeTruthy();

    act(() => {
      colorClass.click();
    });

    const preloader = screen.queryByTestId("preloader") as HTMLElement;
    expect(preloader).toBeTruthy();

    component.unmount();
  });

  it("undo selection when clicking on already selected colorclass", () => {
    Router.navigate("/?colorclass=1");

    const component = render(
      <MemoryRouter>
        <ColorClassFilter
          itemsCount={1}
          subPath={"/test"}
          colorClassActiveList={[1]}
          colorClassUsage={[1, 2]}
        />
      </MemoryRouter>
    );

    const colorClass = screen.queryByTestId(
      "color-class-filter-1"
    ) as HTMLAnchorElement;
    expect(colorClass).toBeTruthy();

    expect(colorClass.classList).toContain("active");

    const urlToStringSpy = jest
      .spyOn(URLPath.prototype, "IUrlToString")
      .mockImplementationOnce(() => {
        return "";
      });

    act(() => {
      colorClass.click();
    });

    expect(urlToStringSpy).toBeCalled();
    expect(urlToStringSpy).toBeCalledWith({ colorClass: [] });

    component.unmount();
    Router.navigate("/");
  });
});
