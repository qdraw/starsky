import { fireEvent, render } from "@testing-library/react";
import { MenuSelectFurther } from "./menu-select-further.tsx";

describe("MenuSelectFurther", () => {
  const toggleLabelsMock = jest.fn();

  afterEach(() => {
    jest.clearAllMocks();
  });

  it('should not render div with class "header" when select prop is not provided', () => {
    const { queryByTestId } = render(
      <MenuSelectFurther toggleLabels={toggleLabelsMock} />
    );
    expect(queryByTestId("select-further")).toBeNull();
  });

  it('should render div with class "header" and handle events when select prop is provided', () => {
    const { getByTestId } = render(
      <MenuSelectFurther select={["item1"]} toggleLabels={toggleLabelsMock} />
    );
    const item = getByTestId("select-further");
    expect(item).toBeTruthy();

    fireEvent.click(item);
    expect(toggleLabelsMock).toHaveBeenCalledTimes(1);

    fireEvent.keyDown(item, { key: "Enter", code: "Enter" });
    expect(toggleLabelsMock).toHaveBeenCalledTimes(2);
  });
});
