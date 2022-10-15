import { globalHistory } from "@reach/router";
import { act, render } from "@testing-library/react";
import React from "react";
import { URLPath } from "../../../shared/url-path";
import ColorClassFilter from "./color-class-filter";

describe("ColorClassFilter", () => {
  it("renders", () => {
    render(
      <ColorClassFilter
        itemsCount={1}
        subPath={"/test"}
        colorClassActiveList={[1, 2]}
        colorClassUsage={[1, 2]}
      ></ColorClassFilter>
    );
  });

  it("onClick value", () => {
    const component = render(
      <ColorClassFilter
        itemsCount={1}
        subPath={"/test"}
        colorClassActiveList={[1]}
        colorClassUsage={[1, 2]}
      />
    );

    const colorClass = component.queryByTestId(
      "color-class-filter-2"
    ) as HTMLAnchorElement;
    expect(colorClass).toBeTruthy();
    colorClass.click();
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
  });

  it("outside current scope display reset", () => {
    const component = render(
      <ColorClassFilter
        itemsCount={1}
        subPath={"/test"}
        colorClassActiveList={[1]}
        colorClassUsage={[3]}
      />
    );

    expect(component.queryByTestId("color-class-filter-reset")).toBeTruthy();
  });

  it("onClick value and preloader exist", () => {
    const component = render(
      <ColorClassFilter
        itemsCount={1}
        subPath={"/test"}
        colorClassActiveList={[1]}
        colorClassUsage={[1, 2]}
      />
    );

    const colorClass = component.queryByTestId(
      "color-class-filter-2"
    ) as HTMLAnchorElement;
    expect(colorClass).toBeTruthy();

    act(() => {
      colorClass.click();
    });

    const preloader = component.queryByTestId("preloader") as HTMLElement;
    expect(preloader).toBeTruthy();

    component.unmount();
  });

  it("undo selection when clicking on already selected colorclass", () => {
    globalHistory.navigate("/?colorclass=1");

    const component = render(
      <ColorClassFilter
        itemsCount={1}
        subPath={"/test"}
        colorClassActiveList={[1]}
        colorClassUsage={[1, 2]}
      />
    );

    const colorClass = component.queryByTestId(
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
    globalHistory.navigate("/");
  });
});
