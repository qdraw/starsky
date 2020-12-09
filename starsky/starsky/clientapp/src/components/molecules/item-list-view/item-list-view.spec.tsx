import { globalHistory } from "@reach/router";
import { mount, shallow } from "enzyme";
import React from "react";
import { act } from "react-dom/test-utils";
import {
  IFileIndexItem,
  newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import { INavigateState } from "../../../interfaces/INavigateState";
import ItemListView from "./item-list-view";
import * as ShiftSelectionHelper from "./shift-selection-helper";

describe("ItemListView", () => {
  it("renders (without state component)", () => {
    shallow(
      <ItemListView
        fileIndexItems={newIFileIndexItemArray()}
        colorClassUsage={[]}
      />
    );
  });

  describe("with Context", () => {
    var exampleData = [
      { fileName: "test.jpg", filePath: "/test.jpg", colorClass: 1 }
    ] as IFileIndexItem[];

    it("search with data-filepath in child element", () => {
      var component = mount(
        <ItemListView fileIndexItems={exampleData} colorClassUsage={[]} />
      );
      var query = '[data-filepath="' + exampleData[0].filePath + '"]';

      expect(component.exists(query)).toBeTruthy();
      component.unmount();
    });

    it("no content", () => {
      var component = shallow(
        <ItemListView fileIndexItems={undefined as any} colorClassUsage={[]} />
      );
      expect(component.text()).toBe("no content");
    });

    it("you did select a different colorclass but there a no items with this colorclass", () => {
      var component = shallow(
        <ItemListView fileIndexItems={[]} colorClassUsage={[2]} />
      );
      expect(component.text()).toBe(
        "There are more items, but these are outside of your filters. To see everything click on 'Reset Filter'"
      );
    });

    it("scroll to state with filePath [item exist]", () => {
      var scrollTo = jest
        .spyOn(window, "scrollTo")
        .mockImplementationOnce(() => {});

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement("div");
      (window as any).domNode = div;
      document.body.appendChild(div);

      globalHistory.location.state = {
        filePath: exampleData[0].filePath
      } as INavigateState;
      jest.useFakeTimers();

      var component = mount(
        <ItemListView fileIndexItems={exampleData} colorClassUsage={[]}>
          item
        </ItemListView>,
        { attachTo: (window as any).domNode }
      );

      act(() => {
        jest.advanceTimersByTime(100);
      });

      expect(scrollTo).toBeCalled();
      expect(scrollTo).toBeCalledWith({ top: 0 });

      jest.clearAllTimers();
      component.unmount();
    });

    it("when clicking shift in selection mode", () => {
      globalHistory.navigate("/?select=");
      var shiftSelectionHelperSpy = jest
        .spyOn(ShiftSelectionHelper, "ShiftSelectionHelper")
        .mockImplementationOnce(() => {
          return true;
        });

      var component = mount(
        <ItemListView fileIndexItems={exampleData} colorClassUsage={[]} />
      );

      act(() => {
        component
          .find(".list-image-box button")
          .simulate("click", { shiftKey: true });
      });

      expect(shiftSelectionHelperSpy).toBeCalled();
      expect(shiftSelectionHelperSpy).toBeCalledWith(
        expect.any(Object),
        [],
        "/test.jpg",
        exampleData
      );
    });
  });
});
