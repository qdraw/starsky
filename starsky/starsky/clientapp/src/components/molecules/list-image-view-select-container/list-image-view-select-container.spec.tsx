import { globalHistory, Link } from "@reach/router";
import { mount, shallow } from "enzyme";
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
    shallow(<ListImageNormalSelectContainer item={fileIndexItem} />);
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
      var component = mount(
        <ListImageNormalSelectContainer item={fileIndexItem}>
          t
        </ListImageNormalSelectContainer>
      );
      component.find(Link).simulate("click", {
        metaKey: false
      });

      expect(component.exists(".preloader--overlay")).toBeTruthy();
      component.unmount();
    });

    it("when click on Link, with command key it should ignore preloader", () => {
      var fileIndexItem = {
        fileName: "test",
        status: IExifStatus.Ok
      } as IFileIndexItem;
      var component = mount(
        <ListImageNormalSelectContainer item={fileIndexItem} />
      );
      component.find(Link).simulate("click", {
        metaKey: true
      });

      expect(component.exists(".preloader--overlay")).toBeFalsy();
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
      var component = mount(
        <ListImageNormalSelectContainer
          item={fileIndexItem}
          onSelectionCallback={onSelectionCallback}
        >
          t
        </ListImageNormalSelectContainer>
      );
      component.find("button").simulate("click", {
        metaKey: false
      });

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
      var component = mount(
        <ListImageNormalSelectContainer
          item={fileIndexItem}
          onSelectionCallback={onSelectionCallback}
        >
          t
        </ListImageNormalSelectContainer>
      );
      component.find("button").simulate("click", {
        shiftKey: true
      });

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

      var component = mount(
        <ListImageNormalSelectContainer
          item={fileIndexItem}
          onSelectionCallback={undefined as any}
        >
          t
        </ListImageNormalSelectContainer>
      );
      component.find("button").simulate("click", {
        shiftKey: true
      });
      // should normal toggle instead of shift action
      expect(globalHistory.location.search).toBe("?select=");
    });
  });
});
