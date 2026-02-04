import { render, screen, fireEvent } from "@testing-library/react";
import { SelectMenuItem } from "./select-menu-item";

describe("SelectMenuItem Component", () => {
  const mockRemoveSidebarSelection = jest.fn();
  const mockToggleLabels = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should render the "Select" button when select is not provided', () => {
    render(
      <SelectMenuItem
        removeSidebarSelection={mockRemoveSidebarSelection}
        toggleLabels={mockToggleLabels}
      />
    );

    const selectButton = screen.getByTestId("menu-item-select");
    expect(selectButton).toBeTruthy();
  });

  it('should call removeSidebarSelection when the "Select" button is clicked', () => {
    render(
      <SelectMenuItem
        removeSidebarSelection={mockRemoveSidebarSelection}
        toggleLabels={mockToggleLabels}
      />
    );

    const selectButton = screen.getByTestId("menu-item-select");
    fireEvent.click(selectButton);
    expect(mockRemoveSidebarSelection).toHaveBeenCalledTimes(1);
  });

  it('should call removeSidebarSelection when "Enter" is pressed on the "Select" button', () => {
    render(
      <SelectMenuItem
        removeSidebarSelection={mockRemoveSidebarSelection}
        toggleLabels={mockToggleLabels}
      />
    );

    const selectButton = screen.getByTestId("menu-item-select");
    fireEvent.keyDown(selectButton, { key: "Enter", code: "Enter" });
    expect(mockRemoveSidebarSelection).toHaveBeenCalledTimes(1);
  });

  it('should render the "Labels" button when select is provided', () => {
    render(
      <SelectMenuItem
        select={["item1"]}
        removeSidebarSelection={mockRemoveSidebarSelection}
        toggleLabels={mockToggleLabels}
      />
    );

    const labelsButton = screen.getByTestId("menu-archive-labels");
    expect(labelsButton).toBeTruthy();
  });

  it('should call toggleLabels when the "Labels" button is clicked', () => {
    render(
      <SelectMenuItem
        select={["item1"]}
        removeSidebarSelection={mockRemoveSidebarSelection}
        toggleLabels={mockToggleLabels}
      />
    );

    const labelsButton = screen.getByTestId("menu-archive-labels");
    fireEvent.click(labelsButton);
    expect(mockToggleLabels).toHaveBeenCalledTimes(1);
  });

  it('should call toggleLabels when "Enter" is pressed on the "Labels" button', () => {
    render(
      <SelectMenuItem
        select={["item1"]}
        removeSidebarSelection={mockRemoveSidebarSelection}
        toggleLabels={mockToggleLabels}
      />
    );

    const labelsButton = screen.getByTestId("menu-archive-labels");
    fireEvent.keyDown(labelsButton, { key: "Enter", code: "Enter" });
    expect(mockToggleLabels).toHaveBeenCalledTimes(1);
  });
});
