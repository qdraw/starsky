import { shallow } from "enzyme";
import React from "react";
import ButtonStyled from "./button-styled";

describe("CurrentLocationButton", () => {
  it("renders", () => {
    shallow(<ButtonStyled />);
  });

  describe("context", () => {
    it("no navigator.geolocation wrong_location", () => {
      var component = shallow(<ButtonStyled type={"submit"} />);
      console.log(component.type);
    });
  });
});
