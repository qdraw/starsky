import { fireEvent, render } from "@testing-library/react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { MenuOptionSelectionAll } from "./menu-option-selection-all";

describe("MenuOptionSelectionAll", () => {
  it("renders", () => {
    render(
      <MenuOptionSelectionAll
        select={["test"]}
        state={{} as IArchiveProps}
        allSelection={() => {}}
      />
    );
  });

  it("renders 2", () => {
    render(
      <MenuOptionSelectionAll
        select={["test"]}
        state={
          {
            fileIndexItems: []
          } as unknown as IArchiveProps
        }
        allSelection={() => {}}
      />
    );
  });

  it("keyDown tab skipped", () => {
    const allSelection = jest.fn();
    const component = render(
      <MenuOptionSelectionAll
        select={["test"]}
        state={
          {
            fileIndexItems: []
          } as unknown as IArchiveProps
        }
        allSelection={allSelection}
      />
    );

    const allItem = component.queryByTestId("select-all") as HTMLElement;
    expect(allItem).toBeTruthy();

    fireEvent.keyDown(allItem, {
      key: "Tab"
    });

    expect(allSelection).toHaveBeenCalledTimes(0);
  });

  it("keyDown enter continue", () => {
    const allSelection = jest.fn();
    const component = render(
      <MenuOptionSelectionAll
        select={["test"]}
        state={
          {
            fileIndexItems: []
          } as unknown as IArchiveProps
        }
        allSelection={allSelection}
      />
    );

    fireEvent.keyDown(component.queryByTestId("select-all") as HTMLElement, {
      key: "Enter"
    });

    expect(allSelection).toHaveBeenCalledTimes(1);
  });

  it("click continue", () => {
    const allSelection = jest.fn();
    const component = render(
      <MenuOptionSelectionAll
        select={["test"]}
        state={
          {
            fileIndexItems: []
          } as unknown as IArchiveProps
        }
        allSelection={allSelection}
      />
    );

    const item = component.queryByTestId("select-all") as HTMLElement;
    item.click();

    expect(allSelection).toHaveBeenCalledTimes(1);
  });
});
