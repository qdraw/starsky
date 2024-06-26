import { fireEvent, render, screen } from "@testing-library/react";
import { LanguageLocalizationExample } from "../../../interfaces/ILanguageLocalization";
import MenuOption from "./menu-option";

describe("MenuOption component", () => {
  it("expect content", () => {
    render(
      <MenuOption
        localization={LanguageLocalizationExample}
        onClickKeydown={() => {}}
        testName="test"
        isReadOnly={false}
      />
    );

    expect(screen.getByTestId("test")).toBeTruthy();
    expect(screen.getByTestId("test").innerHTML).toBe(LanguageLocalizationExample.en);
  });

  it("expect child no localisation field", () => {
    render(
      <MenuOption onClickKeydown={() => {}} testName="test" isReadOnly={false}>
        <div>Content</div>
      </MenuOption>
    );

    expect(screen.getByTestId("test")).toBeTruthy();
    expect(screen.getByTestId("test").innerHTML).toBe("<div>Content</div>");
  });

  it("renders correctly with default props", () => {
    render(
      <MenuOption
        localization={LanguageLocalizationExample}
        onClickKeydown={() => {}}
        testName="test-menu-option"
        isReadOnly={false}
      />
    );

    expect(screen.getByTestId("test-menu-option")).toBeTruthy();
    expect(screen.getByTestId("test-menu-option").innerHTML).toBe("English");
  });

  it("renders correctly with custom props", () => {
    render(
      <MenuOption
        localization={LanguageLocalizationExample}
        onClickKeydown={() => {}}
        testName="test-menu-option1"
        isReadOnly={false}
      />
    );

    expect(screen.getByTestId("test-menu-option1")).toBeTruthy();
  });

  it("calls onClickKeydown when button is clicked", () => {
    const onClickKeydownMock = jest.fn();
    render(
      <MenuOption
        isReadOnly={false}
        localization={LanguageLocalizationExample}
        onClickKeydown={onClickKeydownMock}
        testName="test-menu-option"
      />
    );

    fireEvent.click(screen.getByRole("button"));
    expect(onClickKeydownMock).toHaveBeenCalledTimes(1);
  });

  it("calls onClickKeydown when Enter key is pressed", () => {
    const onClickKeydownMock = jest.fn();
    render(
      <MenuOption
        isReadOnly={false}
        localization={LanguageLocalizationExample}
        onClickKeydown={onClickKeydownMock}
        testName="test-menu-option"
      />
    );

    fireEvent.keyDown(screen.getByRole("button"), { key: "Enter" });
    expect(onClickKeydownMock).toHaveBeenCalledTimes(1);
  });
});
