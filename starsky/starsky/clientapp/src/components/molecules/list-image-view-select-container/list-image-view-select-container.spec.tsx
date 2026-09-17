import { fireEvent, screen } from "@testing-library/dom";
import { render } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { DRAG_MOVE_MIME } from "../../../shared/move-file-helper";
import { Router } from "../../../router-app/router-app";
import ListImageNormalSelectContainer from "./list-image-view-select-container";
describe("ListImageTest", () => {
  it("renders", () => {
    const fileIndexItem = {
      fileName: "test",
      status: IExifStatus.Ok
    } as IFileIndexItem;
    const item = render(
      <MemoryRouter>
        <ListImageNormalSelectContainer item={fileIndexItem} />
      </MemoryRouter>
    );
    expect(item).toBeTruthy();
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

      const anchor = component.container.querySelector("a") as HTMLAnchorElement;
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

      const anchor = component.container.querySelector("a") as HTMLAnchorElement;
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

      const button = component.container.querySelector("button") as HTMLButtonElement;
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

      expect(globalThis.location.search).toBe("?select=test");
      expect(onSelectionCallback).toHaveBeenCalledTimes(0);
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

      const button = component.container.querySelector("button") as HTMLButtonElement;
      expect(button).not.toBeNull();

      fireEvent(
        button,
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
          shiftKey: true
        })
      );

      expect(onSelectionCallback).toHaveBeenCalled();
      expect(onSelectionCallback).toHaveBeenCalledWith("/test.jpg");
      // the update is done in the callback, not here
      expect(globalThis.location.search).toBe("?select=");
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
            onSelectionCallback={undefined as unknown as () => void}
          />
        </MemoryRouter>
      );

      const button = component.container.querySelector("button") as HTMLButtonElement;
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
      expect(globalThis.location.search).toBe("?select=");
    });

    describe("drag-and-drop", () => {
      const dragPayload = { filePaths: ["/test.jpg"], folderPaths: [] };
      const onDragStart = jest.fn(() => dragPayload);

      function makeDragEvent(type: string, withData = false) {
        const stored: Record<string, string> = {};
        const setData = jest.fn((mime: string, val: string) => {
          stored[mime] = val;
        });
        const event = Object.assign(new Event(type, { bubbles: true, cancelable: true }), {
          dataTransfer: {
            types: withData ? [DRAG_MOVE_MIME] : [],
            getData: (mime: string) => stored[mime] ?? "",
            setData,
            dropEffect: "none",
            effectAllowed: "none"
          }
        });
        return { event, setData };
      }

      it("dragStart encodes selection data when onDragStart prop is provided", () => {
        const item = { fileName: "test.jpg", filePath: "/test.jpg", status: IExifStatus.Ok } as IFileIndexItem;
        const { container } = render(
          <MemoryRouter>
            <ListImageNormalSelectContainer item={item} onDragStart={onDragStart} />
          </MemoryRouter>
        );
        const div = container.querySelector("[data-test='list-image-view-select-container']") as HTMLElement;
        const { event, setData } = makeDragEvent("dragstart");
        fireEvent(div, event);
        expect(onDragStart).toHaveBeenCalled();
        expect(setData).toHaveBeenCalledWith(DRAG_MOVE_MIME, JSON.stringify(dragPayload));
      });

      it("dragOver on a directory item with starsky-move data adds drag-over class", () => {
        const item = {
          fileName: "subfolder",
          filePath: "/subfolder",
          isDirectory: true,
          status: IExifStatus.Ok
        } as IFileIndexItem;
        const onDropFiles = jest.fn();
        const { container } = render(
          <MemoryRouter>
            <ListImageNormalSelectContainer item={item} onDragStart={onDragStart} onDropFiles={onDropFiles} />
          </MemoryRouter>
        );
        const div = container.querySelector("[data-test='list-image-view-select-container']") as HTMLElement;
        fireEvent(div, makeDragEvent("dragover", true).event);
        expect(container.querySelector(".box-content--drag-over")).not.toBeNull();
      });

      it("dragLeave on a directory removes drag-over class", () => {
        const item = {
          fileName: "subfolder",
          filePath: "/subfolder",
          isDirectory: true,
          status: IExifStatus.Ok
        } as IFileIndexItem;
        const onDropFiles = jest.fn();
        const { container } = render(
          <MemoryRouter>
            <ListImageNormalSelectContainer item={item} onDragStart={onDragStart} onDropFiles={onDropFiles} />
          </MemoryRouter>
        );
        const div = container.querySelector("[data-test='list-image-view-select-container']") as HTMLElement;
        fireEvent(div, makeDragEvent("dragover", true).event);
        fireEvent(div, makeDragEvent("dragleave").event);
        expect(container.querySelector(".box-content--drag-over")).toBeNull();
      });

      it("drop on directory calls onDropFiles with target folder path", () => {
        const item = {
          fileName: "subfolder",
          filePath: "/subfolder",
          isDirectory: true,
          status: IExifStatus.Ok
        } as IFileIndexItem;
        const onDropFiles = jest.fn();
        const { container } = render(
          <MemoryRouter>
            <ListImageNormalSelectContainer item={item} onDragStart={onDragStart} onDropFiles={onDropFiles} />
          </MemoryRouter>
        );
        const div = container.querySelector("[data-test='list-image-view-select-container']") as HTMLElement;
        fireEvent(div, makeDragEvent("drop", true).event);
        expect(onDropFiles).toHaveBeenCalledWith("/subfolder");
      });

      it("non-directory item does not trigger onDropFiles on drop", () => {
        const item = {
          fileName: "photo.jpg",
          filePath: "/photo.jpg",
          isDirectory: false,
          status: IExifStatus.Ok
        } as IFileIndexItem;
        const onDropFiles = jest.fn();
        const { container } = render(
          <MemoryRouter>
            <ListImageNormalSelectContainer item={item} onDragStart={onDragStart} onDropFiles={onDropFiles} />
          </MemoryRouter>
        );
        const div = container.querySelector("[data-test='list-image-view-select-container']") as HTMLElement;
        fireEvent(div, makeDragEvent("drop", true).event);
        expect(onDropFiles).not.toHaveBeenCalled();
      });
    });
  });
});
