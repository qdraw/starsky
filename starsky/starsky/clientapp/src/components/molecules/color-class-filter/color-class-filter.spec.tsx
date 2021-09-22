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
    var component = render(
      <ColorClassFilter
        itemsCount={1}
        subPath={"/test"}
        colorClassActiveList={[1]}
        colorClassUsage={[1, 2]}
      >
        t
      </ColorClassFilter>
    );
    expect(component.exists(".colorclass--2")).toBeTruthy();
    component.find(".colorclass--2").last().simulate("click");
  });

  it("outside current scope display reset", () => {
    var component = render(
      <ColorClassFilter
        itemsCount={1}
        subPath={"/test"}
        colorClassActiveList={[1]}
        colorClassUsage={[3]}
      >
        t
      </ColorClassFilter>
    );
    expect(component.exists(".colorclass--reset")).toBeTruthy();
  });

  it("onClick value and preloader exist", () => {
    var component = render(
      <ColorClassFilter
        itemsCount={1}
        subPath={"/test"}
        colorClassActiveList={[1]}
        colorClassUsage={[1, 2]}
      >
        t
      </ColorClassFilter>
    );

    expect(component.exists(".colorclass--2")).toBeTruthy();

    component.find(".colorclass--2").last().simulate("click");
    expect(component.exists(".preloader")).toBeTruthy();

    component.unmount();
  });

  it("undo selection when clicking on already selected colorclass", () => {
    globalHistory.navigate("/?colorclass=1");

    var component = render(
      <ColorClassFilter
        itemsCount={1}
        subPath={"/test"}
        colorClassActiveList={[1]}
        colorClassUsage={[1, 2]}
      >
        t
      </ColorClassFilter>
    );

    expect(
      component.find(".colorclass--1").last().hasClass("active")
    ).toBeTruthy();

    var urlToStringSpy = jest
      .spyOn(URLPath.prototype, "IUrlToString")
      .mockImplementationOnce(() => {
        return "";
      });

    act(() => {
      component.find(".colorclass--1").last().simulate("click");
    });

    expect(urlToStringSpy).toBeCalled();
    expect(urlToStringSpy).toBeCalledWith({ colorClass: [] });

    component.unmount();
    globalHistory.navigate("/");
  });
});
