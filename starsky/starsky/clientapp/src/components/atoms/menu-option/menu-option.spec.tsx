import { fireEvent, render, screen } from "@testing-library/react";
import MenuOption from "./menu-option";

describe("MenuOption component", () => {
  it("should not trigger setEnableMoreMenu when isReadOnly is true", () => {
    const setMock = jest.fn();
    const setEnableMoreMenuMock = jest.fn();
    render(
      <MenuOption
        localization={{ nl: "Nederlands", en: "English" }}
        isSet={false}
        set={setMock}
        testName="test"
        isReadOnly={true}
        setEnableMoreMenu={setEnableMoreMenuMock}
      />
    );

    fireEvent.click(screen.getByTestId("test"));

    expect(setMock).toHaveBeenCalledTimes(0);
    expect(setEnableMoreMenuMock).not.toHaveBeenCalled();
  });

  it("should trigger setEnableMoreMenu when isReadOnly is false", () => {
    const setMock = jest.fn();
    const setEnableMoreMenuMock = jest.fn();
    render(
      <MenuOption
        localization={{ nl: "Nederlands", en: "English" }}
        isSet={false}
        set={setMock}
        testName="test"
        isReadOnly={false}
        setEnableMoreMenu={setEnableMoreMenuMock}
      />
    );

    fireEvent.click(screen.getByTestId("test"));

    expect(setMock).toHaveBeenCalledTimes(1);
    expect(setEnableMoreMenuMock).toHaveBeenCalled();
  });
});
