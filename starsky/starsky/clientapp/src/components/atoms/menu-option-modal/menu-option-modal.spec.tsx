import { fireEvent, render, screen } from "@testing-library/react";
import { LanguageLocalizationExample } from "../../../interfaces/ILanguageLocalization.ts";
import MenuOptionModal from "./menu-option-modal.tsx";

describe("MenuOption component", () => {
  it("expect content", () => {
    const setMock = jest.fn();
    const setEnableMoreMenuMock = jest.fn();
    render(
      <MenuOptionModal
        localization={LanguageLocalizationExample}
        isSet={false}
        set={setMock}
        testName="test"
        isReadOnly={true}
        setEnableMoreMenu={setEnableMoreMenuMock}
      />
    );

    expect(screen.getByTestId("test")).toBeTruthy();
    expect(screen.getByTestId("test").innerHTML).toBe(LanguageLocalizationExample.en);
  });

  it("expect child no localisation field", () => {
    const setMock = jest.fn();
    const setEnableMoreMenuMock = jest.fn();
    render(
      <MenuOptionModal
        isSet={false}
        set={setMock}
        testName="test"
        isReadOnly={true}
        setEnableMoreMenu={setEnableMoreMenuMock}
      >
        <div>Content</div>
      </MenuOptionModal>
    );

    expect(screen.getByTestId("test")).toBeTruthy();
    expect(screen.getByTestId("test").innerHTML).toBe("<div>Content</div>");
  });

  it("should not trigger setEnableMoreMenu when isReadOnly is true", () => {
    const setMock = jest.fn();
    const setEnableMoreMenuMock = jest.fn();
    render(
      <MenuOptionModal
        localization={LanguageLocalizationExample}
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
      <MenuOptionModal
        localization={LanguageLocalizationExample}
        isSet={false}
        set={setMock}
        testName="test"
        isReadOnly={false}
        setEnableMoreMenu={setEnableMoreMenuMock}
      />
    );
    expect(screen.getByTestId("test")).toBeTruthy();

    fireEvent.click(screen.getByTestId("test"));

    expect(setMock).toHaveBeenCalledTimes(1);
    expect(setEnableMoreMenuMock).toHaveBeenCalled();
  });

  it("missing localisation field", () => {
    const setMock = jest.fn();
    const setEnableMoreMenuMock = jest.fn();
    render(
      <MenuOptionModal
        isSet={false}
        set={setMock}
        testName="test"
        isReadOnly={false}
        setEnableMoreMenu={setEnableMoreMenuMock}
      />
    );

    expect(screen.getByTestId("test")).toBeTruthy();
    expect(screen.getByTestId("test").innerHTML).toBe("");

    fireEvent.click(screen.getByTestId("test"));

    expect(setMock).toHaveBeenCalledTimes(1);
    expect(setEnableMoreMenuMock).toHaveBeenCalled();
  });
});
