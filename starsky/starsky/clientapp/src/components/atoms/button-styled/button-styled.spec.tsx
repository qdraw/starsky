import { shallow } from "enzyme";
import React from "react";
import ButtonStyled from "./button-styled";

describe("CurrentLocationButton", () => {
  it("renders", () => {
    shallow(<ButtonStyled />);
  });

  describe("ButtonStyled", () => {
    it("set type", () => {
      var component = shallow(<ButtonStyled type={"submit"} />);
      expect(component.find("button").prop("type")).toEqual("submit");
    });
  });
});
