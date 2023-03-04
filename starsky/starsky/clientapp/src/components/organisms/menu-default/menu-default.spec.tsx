import { act, render, screen } from "@testing-library/react";
import MenuDefault from "./menu-default";

describe("MenuDefault", () => {
  it("renders", () => {
    render(<MenuDefault isEnabled={false} />);
  });

  describe("with Context", () => {
    it("has hamburger", () => {
      const component = render(<MenuDefault isEnabled={true} />);
      expect(screen.getByTestId("hamburger")).toBeTruthy();
      component.unmount();
    });

    it("[menu default]check if on click the hamburger opens", () => {
      const component = render(<MenuDefault isEnabled={true} />);

      const hamburger = screen.getByTestId("hamburger");
      expect(hamburger?.querySelector(".open")).toBeFalsy();

      act(() => {
        hamburger?.click();
      });

      expect(hamburger?.querySelector(".open")).toBeTruthy();

      component.unmount();
    });
  });
});
