import { fireEvent, render } from "@testing-library/react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { MenuOptionSelectionUndo } from "./menu-option-selection-undo";

describe("MenuOptionSelectionUndo", () => {
  it("renders", () => {
    const item = render(
      <MenuOptionSelectionUndo
        select={[]}
        state={{} as unknown as IArchiveProps}
        undoSelection={() => {}}
      />
    );
    expect(item).toBeTruthy();
  });

  it("renders 2", () => {
    const item = render(
      <MenuOptionSelectionUndo
        select={[]}
        state={
          {
            fileIndexItems: []
          } as unknown as IArchiveProps
        }
        undoSelection={() => {}}
      />
    );
    expect(item).toBeTruthy();
  });

  it("keyDown tab skipped", () => {
    const undoSelection = jest.fn();
    const component = render(
      <MenuOptionSelectionUndo
        select={[]}
        state={
          {
            fileIndexItems: []
          } as unknown as IArchiveProps
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
          } as unknown as IArchiveProps
        }
        undoSelection={undoSelection}
      />
    );

    fireEvent.keyDown(component.queryByTestId("undo-selection") as HTMLElement, { key: "Enter" });

    expect(undoSelection).toHaveBeenCalledTimes(1);
  });
});
