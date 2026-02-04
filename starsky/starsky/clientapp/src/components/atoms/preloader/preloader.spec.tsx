import { render, screen } from "@testing-library/react";
import Preloader from "./preloader";

describe("Preloader", () => {
  it("renders", () => {
    render(<Preloader isOverlay={false} />);
  });

  it("no overlay", () => {
    const component = render(<Preloader isOverlay={false} />);

    const className = screen.queryByTestId("preloader")?.className;

    expect(className).not.toContain("preloader--overlay");

    component.unmount();
  });

  it("with overlay", () => {
    const component = render(<Preloader isOverlay={true} />);

    const className = screen.queryByTestId("preloader")?.className;

    expect(className).toContain("preloader--overlay");

    component.unmount();
  });
});
