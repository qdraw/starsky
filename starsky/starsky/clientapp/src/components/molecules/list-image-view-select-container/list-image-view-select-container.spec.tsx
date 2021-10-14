import { globalHistory } from "@reach/router";
import { fireEvent, render } from "@testing-library/react";
import React from "react";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import ListImageNormalSelectContainer from "./list-image-view-select-container";

describe("ListImageTest", () => {
  it("renders", () => {
    var fileIndexItem = {
      fileName: "test",
      status: IExifStatus.Ok
    } as IFileIndexItem;
    render(<ListImageNormalSelectContainer item={fileIndexItem} />);
  });

  describe("NonSelectMode", () => {
    beforeAll(() => {
      globalHistory.navigate("/");
    });

    it("NonSelectMode - when click on Link, it should display a preloader", () => {
      var fileIndexItem = {
        fileName: "test",
        status: IExifStatus.Ok
      } as IFileIndexItem;
      var component = render(
        <ListImageNormalSelectContainer item={fileIndexItem}>
          t
        </ListImageNormalSelectContainer>
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

      expect(component.queryByTestId("preloader")).toBeTruthy();
    });

    it("when click on Link, with command key it should ignore preloader", () => {
      var fileIndexItem = {
        fileName: "test",
        status: IExifStatus.Ok
      } as IFileIndexItem;
      var component = render(
        <ListImageNormalSelectContainer item={fileIndexItem} />
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
      globalHistory.navigate("/?select=");
    });

    it("when click on button it add the selected file to the history", () => {
      var fileIndexItem = {
        fileName: "test",
        status: IExifStatus.Ok
      } as IFileIndexItem;

      var onSelectionCallback = jest.fn();
      var component = render(
        <ListImageNormalSelectContainer
          item={fileIndexItem}
          onSelectionCallback={onSelectionCallback}
        >
          t
        </ListImageNormalSelectContainer>
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

      expect(globalHistory.location.search).toBe("?select=test");
      expect(onSelectionCallback).toBeCalledTimes(0);
      component.unmount();
    });

    it("shift click it should submit callback", () => {
      var fileIndexItem = {
        fileName: "test",
        filePath: "/test.jpg",
        status: IExifStatus.Ok
      } as IFileIndexItem;

      var onSelectionCallback = jest.fn();
      var component = render(
        <ListImageNormalSelectContainer
          item={fileIndexItem}
          onSelectionCallback={onSelectionCallback}
        >
          t
        </ListImageNormalSelectContainer>
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
      expect(globalHistory.location.search).toBe("?select=");
    });

    it("shift click it should not submit callback when input is undefined", () => {
      var fileIndexItem = {
        fileName: "test",
        filePath: "/test.jpg",
        status: IExifStatus.Ok
      } as IFileIndexItem;

      var component = render(
        <ListImageNormalSelectContainer
          item={fileIndexItem}
          onSelectionCallback={undefined as any}
        >
          t
        </ListImageNormalSelectContainer>
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
      expect(globalHistory.location.search).toBe("?select=");
    });
  });
});
