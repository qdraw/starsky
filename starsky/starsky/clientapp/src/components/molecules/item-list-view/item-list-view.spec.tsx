import { globalHistory } from "@reach/router";
import { mount, shallow } from "enzyme";
import { act } from "react-dom/test-utils";
import {
  IFileIndexItem,
  newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import { INavigateState } from "../../../interfaces/INavigateState";
import * as FlatListItem from "../../atoms/flat-list-item/flat-list-item";
import * as ListImageChildItem from "../../atoms/list-image-child-item/list-image-child-item";
import ItemListView from "./item-list-view";
import * as ShiftSelectionHelper from "./shift-selection-helper";

describe("ItemListView", () => {
  it("renders (without state component)", () => {
    shallow(
      <ItemListView
        iconList={true}
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
        <ItemListView
          iconList={true}
          fileIndexItems={exampleData}
          colorClassUsage={[]}
        />
      );
      var query = '[data-filepath="' + exampleData[0].filePath + '"]';

      expect(component.exists(query)).toBeTruthy();
      component.unmount();
    });

    it("should return FlatListItem when iconList is false", () => {
      const flatListItemSpy = jest
        .spyOn(FlatListItem, "default")
        .mockImplementationOnce(() => <></>);
      var component = mount(
        <ItemListView
          iconList={false}
          fileIndexItems={exampleData}
          colorClassUsage={[]}
        />
      );
      expect(flatListItemSpy).toBeCalled();
      component.unmount();
    });

    it("no content", () => {
      var component = shallow(
        <ItemListView
          iconList={true}
          fileIndexItems={undefined as any}
          colorClassUsage={[]}
        />
      );
      expect(component.text()).toBe("no content");
    });

    it("text should be: New? Set your drive location in the settings.  There are no photos in this folder", () => {
      var component = shallow(
        <ItemListView
          iconList={true}
          fileIndexItems={[]}
          subPath="/"
          colorClassUsage={[]}
        />
      );
      expect(component.text()).toBe(
        "New? Set your drive location in the settings.  There are no photos in this folder"
      );
    });

    it("text should be: There are no photos in this folder", () => {
      var component = shallow(
        <ItemListView
          iconList={true}
          fileIndexItems={[]}
          subPath="/test"
          colorClassUsage={[]}
        />
      );
      expect(component.text()).toBe("There are no photos in this folder");
    });

    it("you did select a different colorclass but there a no items with this colorclass", () => {
      var component = shallow(
        <ItemListView
          iconList={true}
          fileIndexItems={[]}
          colorClassUsage={[2]}
        />
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
        <ItemListView
          iconList={true}
          fileIndexItems={exampleData}
          colorClassUsage={[]}
        >
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
      const listImageChildItemSpy = jest.spyOn(ListImageChildItem, "default");
      globalHistory.navigate("/?select=");
      var shiftSelectionHelperSpy = jest
        .spyOn(ShiftSelectionHelper, "ShiftSelectionHelper")
        .mockImplementationOnce(() => {
          return true;
        });

      var component = mount(
        <ItemListView
          iconList={true}
          fileIndexItems={exampleData}
          colorClassUsage={[]}
        />
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
      expect(listImageChildItemSpy).toBeCalled();
    });

    it("should return ListImageChildItem when iconList is true", () => {
      // should be last in list
      const listImageChildItemSpy = jest
        .spyOn(ListImageChildItem, "default")
        .mockImplementationOnce(() => <>t</>);
      var component = mount(
        <ItemListView
          iconList={true}
          fileIndexItems={exampleData}
          colorClassUsage={[]}
        />
      );
      expect(listImageChildItemSpy).toBeCalled();
      component.unmount();
      jest.spyOn(ListImageChildItem, "default").mockReset();
    });
  });
});
