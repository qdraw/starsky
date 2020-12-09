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
});
