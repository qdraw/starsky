import { mount, shallow } from "enzyme";
import React from "react";
import * as MenuDefault from "../menu-default/menu-default";
import ApplicationException from "./application-exception";

describe("ApplicationException", () => {
  it("renders", () => {
    shallow(<ApplicationException>t</ApplicationException>);
  });

  it("should have menu", () => {
    const menuDefaultSpy = jest
      .spyOn(MenuDefault, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    const component = mount(<ApplicationException>t</ApplicationException>);

    expect(menuDefaultSpy).toBeCalled();

    component.unmount();
  });

  it("should have warning", () => {
    jest.spyOn(MenuDefault, "default").mockImplementationOnce(() => {
      return <></>;
    });

    const component = mount(<ApplicationException>t</ApplicationException>);

    expect(component.exists(".content--header")).toBeTruthy();

    component.unmount();
  });

  it("click on reload", () => {
    const reloadSpy = jest
      .spyOn(window.location, "reload")
      .mockImplementationOnce(() => {});

    jest.spyOn(MenuDefault, "default").mockImplementationOnce(() => {
      return <></>;
    });

    const component = mount(<ApplicationException>t</ApplicationException>);

    component.find("[data-test='reload']").simulate("click");

    expect(reloadSpy).toBeCalled();

    jest.spyOn(window.location, "reload").mockRestore();
    component.unmount();
  });
});
