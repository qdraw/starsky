import { render } from "@testing-library/react";
import React from "react";
import Preloader from "./preloader";

describe("Preloader", () => {
  it("renders", () => {
    render(<Preloader isOverlay={false} />);
  });

  it("no overlay", () => {
    var component = render(<Preloader isOverlay={false} />);
    expect(component.exists(".preloader--overlay")).toBeFalsy();
  });

  it("with overlay", () => {
    var component = render(<Preloader isOverlay={true} />);
    expect(component.exists(".preloader--overlay")).toBeTruthy();
  });
});
