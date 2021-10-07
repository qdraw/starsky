import { render } from "@testing-library/react";
import React from "react";
import Preloader from "./preloader";

describe("Preloader", () => {
  it("renders", () => {
    render(<Preloader isOverlay={false} />);
  });

  it("no overlay", () => {
    const component = render(<Preloader isOverlay={false} />);
    console.log(component.queryByTestId("preloader"));

    const className = component.queryByTestId("preloader")?.className;

    expect(className).toContain("preloader--overlay");
  });

  it("with overlay", () => {
    const component = render(<Preloader isOverlay={true} />);
    expect(component.exists(".preloader--overlay")).toBeTruthy();
  });
});
