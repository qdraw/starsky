import { render, screen } from "@testing-library/react";
import ButtonStyled from "./button-styled";

describe("CurrentLocationButton", () => {
  it("renders", () => {
    render(<ButtonStyled />);
  });

  describe("ButtonStyled", () => {
    it("set type", () => {
      const component = render(<ButtonStyled type={"submit"} />);
      const button = screen.getByRole("button") as HTMLInputElement;
      expect(button.type).toEqual("submit");
      component.unmount();
    });
  });
});
