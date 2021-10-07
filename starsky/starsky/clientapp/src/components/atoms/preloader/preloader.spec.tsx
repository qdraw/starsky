import { render } from "@testing-library/react";
import React from "react";
import Preloader from "./preloader";

describe("Preloader", () => {
  it("renders", () => {
    render(<Preloader isOverlay={false} />);
  });

  it("no overlay", () => {
    const component = render(<Preloader isOverlay={false} />);

    const className = component.queryByTestId("preloader")?.className;

    expect(className).not.toContain("preloader--overlay");
  });

  it("with overlay", () => {
    const component = render(<Preloader isOverlay={true} />);

    const className = component.queryByTestId("preloader")?.className;

    expect(className).toContain("preloader--overlay");
  });
});
