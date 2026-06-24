import { fireEvent, screen, waitFor } from "@testing-library/dom";
import { render } from "@testing-library/react";
import Tooltip from "./tooltip";

describe("Tooltip component", () => {
  it("renders tooltip text on hover", async () => {
    const text = "This is a tooltip";
    const component = render(
      <Tooltip text={text} left={false}>
        Hover me
      </Tooltip>
    );

    // Tooltip should not be visible initially
    expect(screen.queryByTestId("tooltip")).toBeFalsy();

    // Use mouseOver to trigger React onMouseEnter in jsdom
    fireEvent.mouseOver(screen.getByText("Hover me"));

    // Tooltip should now be visible
    await waitFor(() => {
      expect(screen.getByTestId("tooltip")).toBeTruthy();
      expect(screen.getByText(text)).toBeTruthy();
    });

    // Move mouse away to hide the tooltip
    fireEvent.mouseOut(screen.getByText("Hover me"));
    await waitFor(() => {
      expect(screen.queryByTestId("tooltip")).toBeFalsy();
    });
    component.unmount();
  });

  describe("click behavior", () => {
    it("renders tooltip text on click", () => {
      const text = "This is a tooltip";
      const component = render(
        <Tooltip text={text} left={false}>
          Click me
        </Tooltip>
      );

      const test = component.container.innerHTML;
      console.log(test);

      // Tooltip should not be visible initially
      expect(screen.queryByTestId("tooltip")).toBeFalsy();

      // Click on the button to display the tooltip
      fireEvent.click(screen.getByText("Click me"));

      // Tooltip should now be visible
      expect(screen.queryByTestId("tooltip")).toBeTruthy();
      expect(screen.getByText(text)).toBeTruthy();

      // Click on the button again to hide the tooltip
      fireEvent.click(screen.getByText("Click me"));

      expect(screen.queryByTestId("tooltip")).toBeFalsy();

      component.unmount();
    });
  });
});
