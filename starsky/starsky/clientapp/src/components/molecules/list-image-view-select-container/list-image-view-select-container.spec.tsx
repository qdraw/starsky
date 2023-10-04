import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { Router } from "../../../router-app/router-app";
import ListImageNormalSelectContainer from "./list-image-view-select-container";
describe("ListImageTest", () => {
  it("renders", () => {
    const fileIndexItem = {
      fileName: "test",
      status: IExifStatus.Ok
    } as IFileIndexItem;
    render(
      <MemoryRouter>
        <ListImageNormalSelectContainer item={fileIndexItem} />
      </MemoryRouter>
    );
  });

  describe("NonSelectMode", () => {
    beforeAll(() => {
      Router.navigate("/");
    });

    it("NonSelectMode - when click on Link, it should display a preloader", () => {
      const fileIndexItem = {
        fileName: "test",
        status: IExifStatus.Ok
      } as IFileIndexItem;
      const component = render(
        <MemoryRouter>
          <ListImageNormalSelectContainer item={fileIndexItem} />
        </MemoryRouter>
      );

      const anchor = component.container.querySelector(
        "a"
      ) as HTMLAnchorElement;
      expect(anchor).not.toBeNull();

      fireEvent(
        anchor,
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
          metaKey: false
        })
      );

      expect(screen.getByTestId("preloader")).toBeTruthy();
    });

    it("when click on Link, with command key it should ignore preloader", () => {
      const fileIndexItem = {
        fileName: "test",
        status: IExifStatus.Ok
      } as IFileIndexItem;
      const component = render(
        <MemoryRouter>
          <ListImageNormalSelectContainer item={fileIndexItem} />
        </MemoryRouter>
      );

      const anchor = component.container.querySelector(
        "a"
      ) as HTMLAnchorElement;
      expect(anchor).not.toBeNull();

      fireEvent(
        anchor,
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
          metaKey: true
        })
      );

      expect(component.queryByTestId("preloader")).toBeFalsy();

      component.unmount();
    });
  });

  describe("SelectMode", () => {
    beforeEach(() => {
      Router.navigate("/?select=");
    });

    it("when click on button it add the selected file to the history", () => {
      const fileIndexItem = {
        fileName: "test",
        status: IExifStatus.Ok
      } as IFileIndexItem;

      const onSelectionCallback = jest.fn();
      const component = render(
        <MemoryRouter>
          <ListImageNormalSelectContainer
            item={fileIndexItem}
            onSelectionCallback={onSelectionCallback}
          />
        </MemoryRouter>
      );

      const button = component.container.querySelector(
        "button"
      ) as HTMLButtonElement;
      expect(button).not.toBeNull();

      // ClickEvent
      fireEvent(
        button,
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
          metaKey: false
        })
      );

      expect(window.location.search).toBe("?select=test");
      expect(onSelectionCallback).toBeCalledTimes(0);
      component.unmount();
    });

    it("shift click it should submit callback", () => {
      const fileIndexItem = {
        fileName: "test",
        filePath: "/test.jpg",
        status: IExifStatus.Ok
      } as IFileIndexItem;

      const onSelectionCallback = jest.fn();
      const component = render(
        <MemoryRouter>
          <ListImageNormalSelectContainer
            item={fileIndexItem}
            onSelectionCallback={onSelectionCallback}
          />
        </MemoryRouter>
      );

      const button = component.container.querySelector(
        "button"
      ) as HTMLButtonElement;
      expect(button).not.toBeNull();

      fireEvent(
        button,
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
          shiftKey: true
        })
      );

      expect(onSelectionCallback).toBeCalled();
      expect(onSelectionCallback).toBeCalledWith("/test.jpg");
      // the update is done in the callback, not here
      expect(window.location.search).toBe("?select=");
    });

    it("shift click it should not submit callback when input is undefined", () => {
      const fileIndexItem = {
        fileName: "test",
        filePath: "/test.jpg",
        status: IExifStatus.Ok
      } as IFileIndexItem;

      const component = render(
        <MemoryRouter>
          <ListImageNormalSelectContainer
            item={fileIndexItem}
            onSelectionCallback={undefined as any}
          />
        </MemoryRouter>
      );

      const button = component.container.querySelector(
        "button"
      ) as HTMLButtonElement;
      expect(button).not.toBeNull();

      fireEvent(
        button,
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
          shiftKey: true
        })
      );

      // should normal toggle instead of shift action
      expect(window.location.search).toBe("?select=");
    });
  });
});
