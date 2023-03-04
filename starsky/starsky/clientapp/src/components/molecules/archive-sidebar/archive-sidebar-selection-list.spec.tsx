import { globalHistory } from "@reach/router";
import { act, render, screen } from "@testing-library/react";
import {
  IFileIndexItem,
  newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import { URLPath } from "../../../shared/url-path";
import ArchiveSidebarSelectionList from "./archive-sidebar-selection-list";

describe("archive-sidebar-selection-list", () => {
  it("renders", () => {
    render(
      <ArchiveSidebarSelectionList fileIndexItems={newIFileIndexItemArray()} />
    );
  });

  describe("with select state", () => {
    beforeEach(() => {
      jest.mock("@reach/router", () => ({
        navigate: jest.fn(),
        globalHistory: jest.fn()
      }));

      act(() => {
        // to use with: => import { act } from 'react-dom/test-utils';
        globalHistory.navigate("/?select=test.jpg");
      });
    });

    const items = [
      { fileName: "test.jpg", parentDirectory: "/" },
      { fileName: "to-select.jpg", parentDirectory: "/" }
    ] as IFileIndexItem[];

    it("with items, check first item", () => {
      const component = render(
        <ArchiveSidebarSelectionList fileIndexItems={items} />
      );

      const selectionList = screen.queryByTestId(
        "sidebar-selection-list"
      ) as HTMLElement;

      expect(selectionList.children[0].textContent).toBe("test.jpg");

      component.unmount();
    });

    it("toggleSelection", () => {
      const component = render(
        <ArchiveSidebarSelectionList fileIndexItems={items} />
      );

      const spy = jest.spyOn(URLPath.prototype, "toggleSelection");

      const selectionList = screen.queryByTestId(
        "sidebar-selection-list"
      ) as HTMLElement;

      act(() => {
        (
          selectionList.children[0].querySelector(".close") as HTMLElement
        ).click();
      });

      expect(spy).toBeCalledTimes(1);

      spy.mockClear();

      component.unmount();
    });

    it("allSelection", () => {
      const component = render(
        <ArchiveSidebarSelectionList fileIndexItems={items} />
      );
      const allSelectionButton = screen.queryByTestId("select-all");

      const spy = jest.spyOn(URLPath.prototype, "GetAllSelection");

      act(() => {
        (allSelectionButton as HTMLElement).click();
      });

      expect(spy).toBeCalledTimes(1);

      spy.mockClear();

      component.unmount();
    });
  });
});
