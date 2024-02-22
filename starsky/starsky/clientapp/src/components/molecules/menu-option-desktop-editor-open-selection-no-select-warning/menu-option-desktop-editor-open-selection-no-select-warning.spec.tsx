import { fireEvent, render, screen } from "@testing-library/react";
import MenuOptionDesktopEditorOpenSelectionNoSelectWarning from "./menu-option-desktop-editor-open-selection-no-select-warning";

describe("MenuOptionDesktopEditorOpenSelectionNoSelectWarning", () => {
  it("should render without crashing", () => {
    render(<MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={false} />);
  });

  it("should show error notification when trying to open editor without selecting anything", () => {
    render(<MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={false} />);
    fireEvent.keyDown(window, { key: "e", ctrlKey: true });
    expect(screen.getByText("select first")).toBeTruthy();
  });

  it("should not show error notification when select is not empty", () => {
    render(
      <MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={["item"]} isReadOnly={false} />
    );
    fireEvent.keyDown(window, { key: "e", ctrlKey: true });
    expect(screen.queryByText("select first")).toBeNull();
  });

  it("should not show error notification when read-only mode is enabled", () => {
    render(<MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={true} />);
    fireEvent.keyDown(window, { key: "e", ctrlKey: true });
    expect(screen.queryByText("select first")).toBeNull();
  });

  it("should not show error notification when editor feature is disabled", () => {
    jest.spyOn(window, "fetch").mockImplementation(() => ({
      json: async () => ({ openEditorEnabled: false })
    }));
    render(<MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={false} />);
    fireEvent.keyDown(window, { key: "e", ctrlKey: true });
    expect(screen.queryByText("select first")).toBeNull();
  });

  it("should clear error notification on callback", () => {
    render(<MenuOptionDesktopEditorOpenSelectionNoSelectWarning select={[]} isReadOnly={false} />);
    fireEvent.keyDown(window, { key: "e", ctrlKey: true });
    fireEvent.click(screen.getByText("Close")); // Assuming 'Close' is the text inside the notification's close button
    expect(screen.queryByText("select first")).not.toBeInTheDocument();
  });
});
