import { fireEvent, render } from "@testing-library/react";
import { MenuOptionSelectionUndo } from "./menu-option-selection-undo";

describe("MenuOptionSelectionUndo", () => {
  it("renders", () => {
    render(<MenuOptionSelectionUndo select={[]} state={{} as any} undoSelection={() => {}} />);
  });

  it("renders 2", () => {
    render(
      <MenuOptionSelectionUndo
        select={[]}
        state={
          {
            fileIndexItems: []
          } as any
        }
        undoSelection={() => {}}
      />
    );
  });

  it("keyDown tab skipped", () => {
    const undoSelection = jest.fn();
    const component = render(
      <MenuOptionSelectionUndo
        select={[]}
        state={
          {
            fileIndexItems: []
          } as any
        }
        undoSelection={undoSelection}
      />
    );

    fireEvent.keyDown(component.queryByTestId("undo-selection") as HTMLElement, { key: "Tab" });

    expect(undoSelection).toHaveBeenCalledTimes(0);
  });

  it("keyDown enter continue", () => {
    const undoSelection = jest.fn();
    const component = render(
      <MenuOptionSelectionUndo
        select={[]}
        state={
          {
            fileIndexItems: []
          } as any
        }
        undoSelection={undoSelection}
      />
    );

    fireEvent.keyDown(component.queryByTestId("undo-selection") as HTMLElement, { key: "Enter" });

    expect(undoSelection).toHaveBeenCalledTimes(1);
  });
});
