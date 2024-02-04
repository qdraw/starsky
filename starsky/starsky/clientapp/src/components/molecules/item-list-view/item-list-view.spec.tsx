import { fireEvent, render, screen } from "@testing-library/react";
import { act } from "react-dom/test-utils";
import { MemoryRouter } from "react-router-dom";
import { IUseLocation } from "../../../hooks/use-location/interfaces/IUseLocation";
import * as useLocation from "../../../hooks/use-location/use-location";
import { IFileIndexItem, newIFileIndexItemArray } from "../../../interfaces/IFileIndexItem";
import { Router } from "../../../router-app/router-app";
import * as FlatListItem from "../../atoms/flat-list-item/flat-list-item";
import * as ListImageChildItem from "../../atoms/list-image-child-item/list-image-child-item";
import ItemListView from "./item-list-view";
import * as ShiftSelectionHelper from "./internal/shift-selection-helper";

describe("ItemListView", () => {
  it("renders (without state component)", () => {
    render(
      <ItemListView
        iconList={true}
        fileIndexItems={newIFileIndexItemArray()}
        colorClassUsage={[]}
      />
    );
  });

  describe("with Context", () => {
    const exampleData = [
      { fileName: "test.jpg", filePath: "/test.jpg", colorClass: 1 }
    ] as IFileIndexItem[];

    it("search with data-filepath in child element", () => {
      const component = render(
        <MemoryRouter>
          <ItemListView iconList={true} fileIndexItems={exampleData} colorClassUsage={[]} />
        </MemoryRouter>
      );

      const element = screen.queryAllByTestId("list-image-view-select-container")[0];

      expect(element).toBeTruthy();

      expect(element.dataset["filepath"]).toBeTruthy();
      expect(element.dataset["filepath"]).toBe(exampleData[0].filePath);

      component.unmount();
    });

    it("should return FlatListItem when iconList is false", () => {
      const flatListItemSpy = jest
        .spyOn(FlatListItem, "default")
        .mockImplementationOnce(() => <></>);
      const component = render(
        <MemoryRouter>
          <ItemListView iconList={false} fileIndexItems={exampleData} colorClassUsage={[]} />
        </MemoryRouter>
      );
      expect(flatListItemSpy).toBeCalled();
      component.unmount();
    });

    it("no content", () => {
      const component = render(
        <MemoryRouter>
          <ItemListView iconList={true} fileIndexItems={undefined as any} colorClassUsage={[]} />
        </MemoryRouter>
      );
      expect(component.container.textContent).toBe("no content");
    });

    it("text should be: New? Set your drive location in the settings. There are no photos in this folder", () => {
      const component = render(
        <ItemListView iconList={true} fileIndexItems={[]} subPath="/" colorClassUsage={[]} />
      );
      expect(component.container.textContent).toBe(
        "New? Set your drive location in the settings. There are no photos in this folder"
      );
    });

    it("text should be: There are no photos in this folder", () => {
      const component = render(
        <ItemListView iconList={true} fileIndexItems={[]} subPath="/test" colorClassUsage={[]} />
      );
      expect(component.container.textContent).toBe("There are no photos in this folder");
    });

    it("you did select a different colorclass but there a no items with this colorclass", () => {
      const component = render(
        <ItemListView iconList={true} fileIndexItems={[]} colorClassUsage={[2]} />
      );
      expect(component.container.textContent).toBe(
        "There are more items, but these are outside of your filters. To see everything click on 'Reset Filter'"
      );
    });

    it("scroll to state with filePath [item exist]", () => {
      const scrollTo = jest
        .spyOn(window, "scrollTo")
        .mockReset()
        .mockImplementationOnce(() => {});

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement("div");
      (window as any).domNode = div;
      document.body.appendChild(div);

      const useLocationMock = {
        location: {
          state: {
            filePath: exampleData[0].filePath
          }
        },
        navigate: jest.fn()
      } as unknown as IUseLocation;

      jest.spyOn(useLocation, "default").mockImplementationOnce(() => useLocationMock);

      jest.useFakeTimers();

      const component = render(
        <MemoryRouter>
          <ItemListView
            iconList={true}
            fileIndexItems={exampleData}
            colorClassUsage={[]}
          ></ItemListView>
        </MemoryRouter>
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
      Router.navigate("/?select=");
      const shiftSelectionHelperSpy = jest
        .spyOn(ShiftSelectionHelper, "ShiftSelectionHelper")
        .mockImplementationOnce(() => true);

      const component = render(
        <MemoryRouter>
          <ItemListView iconList={true} fileIndexItems={exampleData} colorClassUsage={[]} />
        </MemoryRouter>
      );

      const item = screen.queryByTestId("list-image-view-select-container") as HTMLButtonElement;

      console.log(component.container.innerHTML);
      expect(item).toBeTruthy();

      const button = item.querySelector("button") as HTMLButtonElement;
      expect(button).toBeTruthy();

      fireEvent(
        button,
        new MouseEvent("click", {
          bubbles: true,
          cancelable: true,
          shiftKey: true
        })
      );

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
      const component = render(
        <MemoryRouter>
          <ItemListView iconList={true} fileIndexItems={exampleData} colorClassUsage={[]} />
        </MemoryRouter>
      );
      expect(listImageChildItemSpy).toBeCalled();
      component.unmount();
      jest.spyOn(ListImageChildItem, "default").mockReset();
    });
  });
});
