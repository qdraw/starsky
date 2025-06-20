import { createEvent, fireEvent, render, screen } from "@testing-library/react";
import { act } from "react";
import { IFileIndexItem, newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import { Router } from "../../../router-app/router-app";
import { URLPath } from "../../../shared/url/url-path";
import ArchiveSidebarSelectionList from "./archive-sidebar-selection-list";

describe("archive-sidebar-selection-list", () => {
  it("renders", () => {
    render(<ArchiveSidebarSelectionList fileIndexItems={newIFileIndexItemArray()} />);
  });

  describe("with select state", () => {
    beforeEach(() => {
      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        Router.navigate("/?select=test.jpg");
      });
    });

    const items = [
      { fileName: "test.jpg", parentDirectory: "/" },
      { fileName: "to-select.jpg", parentDirectory: "/" }
    ] as IFileIndexItem[];

    it("with items, check first item", () => {
      const component = render(<ArchiveSidebarSelectionList fileIndexItems={items} />);

      const selectionList = screen.queryByTestId("sidebar-selection-list") as HTMLElement;

      expect(selectionList.children[0].textContent).toBe("test.jpg");

      component.unmount();
    });

    it("toggleSelection", () => {
      const component = render(<ArchiveSidebarSelectionList fileIndexItems={items} />);

      const spy = jest.spyOn(URLPath.prototype, "toggleSelection");

      const selectionList = screen.queryByTestId("sidebar-selection-list") as HTMLElement;

      act(() => {
        (selectionList.children[0].querySelector(".close") as HTMLElement).click();
      });

      expect(spy).toHaveBeenCalledTimes(1);

      spy.mockClear();

      component.unmount();
    });

    it("toggleSelection keyboard keyDown it hits", () => {
      const component = render(<ArchiveSidebarSelectionList fileIndexItems={items} />);

      const spy = jest.spyOn(URLPath.prototype, "toggleSelection");

      const selectionList = screen.queryByTestId("sidebar-selection-list") as HTMLElement;
      const element = selectionList.children[0].querySelector(".close") as HTMLElement;

      act(() => {
        const inputEvent = createEvent.keyDown(element, { key: "Enter" });
        fireEvent(element, inputEvent);
      });

      expect(spy).toHaveBeenCalledTimes(1);

      spy.mockClear();

      component.unmount();
    });

    it("toggleSelection keyboard keyDown it ignores", () => {
      const component = render(<ArchiveSidebarSelectionList fileIndexItems={items} />);

      const spy = jest.spyOn(URLPath.prototype, "toggleSelection");

      const selectionList = screen.queryByTestId("sidebar-selection-list") as HTMLElement;
      const element = selectionList.children[0].querySelector(".close") as HTMLElement;

      act(() => {
        const inputEvent = createEvent.keyDown(element, { key: "Tab" });
        fireEvent(element, inputEvent);
      });

      expect(spy).toHaveBeenCalledTimes(0);

      spy.mockClear();

      component.unmount();
    });

    it("allSelection", () => {
      const component = render(<ArchiveSidebarSelectionList fileIndexItems={items} />);
      const allSelectionButton = screen.queryByTestId("select-all");

      const spy = jest.spyOn(URLPath.prototype, "GetAllSelection");

      act(() => {
        (allSelectionButton as HTMLElement).click();
      });

      expect(spy).toHaveBeenCalledTimes(1);

      spy.mockClear();

      component.unmount();
    });
  });
});
