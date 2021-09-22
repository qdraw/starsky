import { render } from "@testing-library/react";
import React from "react";
import ButtonStyled from "./button-styled";

describe("CurrentLocationButton", () => {
  it("renders", () => {
    render(<ButtonStyled />);
  });

  describe("ButtonStyled", () => {
    it("set type", () => {
      var component = render(<ButtonStyled type={"submit"} />);
      const button = component.getByRole("button") as HTMLInputElement;
      expect(button.type).toEqual("submit");
    });
  });
});
