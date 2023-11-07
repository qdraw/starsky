import { fireEvent, render } from "@testing-library/react";
import { MenuSelectCount } from "./menu-select-count";

describe("MenuOptionSelectionAll", () => {
  it("renders", () => {
    render(
      <MenuSelectCount select={["test"]} removeSidebarSelection={() => {}} />
    );
  });

  it("selected-0 keyDown tab skipped", () => {
    const removeSidebarSelection = jest.fn();
    const component = render(
      <MenuSelectCount
        select={[]}
        removeSidebarSelection={removeSidebarSelection}
      />
    );

    const allItem = component.queryByTestId("selected-0") as HTMLElement;
    expect(allItem).toBeTruthy();

    fireEvent.keyDown(allItem, {
      key: "Tab"
    });

    expect(removeSidebarSelection).toBeCalledTimes(0);
  });

  it("selected-0 keyDown enter continue", () => {
    const removeSidebarSelection = jest.fn();
    const component = render(
      <MenuSelectCount
        select={[]}
        removeSidebarSelection={removeSidebarSelection}
      />
    );

    fireEvent.keyDown(component.queryByTestId("selected-0") as HTMLElement, {
      key: "Enter"
    });

    expect(removeSidebarSelection).toBeCalledTimes(1);
  });

  it("selected-0 click continue", () => {
    const removeSidebarSelection = jest.fn();
    const component = render(
      <MenuSelectCount
        select={[]}
        removeSidebarSelection={removeSidebarSelection}
      />
    );

    const item = component.queryByTestId("selected-0") as HTMLElement;
    item.click();

    expect(removeSidebarSelection).toBeCalledTimes(1);
  });

  it("selected-1 keyDown tab skipped", () => {
    const removeSidebarSelection = jest.fn();
    const component = render(
      <MenuSelectCount
        select={["1"]}
        removeSidebarSelection={removeSidebarSelection}
      />
    );

    const allItem = component.queryByTestId("selected-1") as HTMLElement;
    expect(allItem).toBeTruthy();

    fireEvent.keyDown(allItem, {
      key: "Tab"
    });

    expect(removeSidebarSelection).toBeCalledTimes(0);
  });

  it("selected-1 keyDown enter continue", () => {
    const removeSidebarSelection = jest.fn();
    const component = render(
      <MenuSelectCount
        select={["1"]}
        removeSidebarSelection={removeSidebarSelection}
      />
    );

    fireEvent.keyDown(component.queryByTestId("selected-1") as HTMLElement, {
      key: "Enter"
    });

    expect(removeSidebarSelection).toBeCalledTimes(1);
  });
});
