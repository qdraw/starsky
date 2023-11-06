import { fireEvent, render } from "@testing-library/react";
import { MenuOptionSelectionAll } from "./menu-option-selection-all";

describe("MenuOptionSelectionAll", () => {
  it("renders", () => {
    render(
      <MenuOptionSelectionAll
        select={[]}
        state={{} as any}
        allSelection={() => {}}
      />
    );
  });

  it("renders 2", () => {
    render(
      <MenuOptionSelectionAll
        select={[]}
        state={
          {
            fileIndexItems: []
          } as any
        }
        allSelection={() => {}}
      />
    );
  });

  it("keyDown tab skipped", () => {
    const allSelection = jest.fn();
    const component = render(
      <MenuOptionSelectionAll
        select={[]}
        state={
          {
            fileIndexItems: []
          } as any
        }
        allSelection={allSelection}
      />
    );
    console.log(component.container.innerHTML);

    const allItem = component.queryByTestId("select-all") as HTMLElement;
    expect(allItem).toBeTruthy();

    fireEvent.keyDown(allItem, {
      key: "Tab"
    });

    expect(allSelection).toBeCalledTimes(0);
  });

  it("keyDown enter continue", () => {
    const allSelection = jest.fn();
    const component = render(
      <MenuOptionSelectionAll
        select={[]}
        state={
          {
            fileIndexItems: []
          } as any
        }
        allSelection={allSelection}
      />
    );

    fireEvent.keyDown(
      component.queryByTestId("undo-selection") as HTMLElement,
      { key: "Enter" }
    );

    expect(allSelection).toBeCalledTimes(1);
  });
});
