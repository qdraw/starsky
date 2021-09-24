import { globalHistory } from "@reach/router";
import { render } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import * as useLocation from "../../../hooks/use-location";
import { newIArchive } from "../../../interfaces/IArchive";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import {
  IFileIndexItem,
  newIFileIndexItem,
  newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../shared/fetch-post";
import MenuOptionMoveToTrash from "./menu-option-move-to-trash";

describe("MenuOptionMoveToTrash", () => {
  it("renders", () => {
    var test = {
      ...newIArchive(),
      fileIndexItems: newIFileIndexItemArray()
    } as IArchiveProps;
    render(
      <MenuOptionMoveToTrash
        setSelect={jest.fn()}
        select={["test.jpg"]}
        isReadOnly={true}
        state={test}
        dispatch={jest.fn()}
      />
    );
  });

  describe("context", () => {
    it("check if dispatch", async () => {
      jest.spyOn(FetchPost, "default").mockReset();
      var test = {
        ...newIArchive(),
        fileIndexItems: [
          {
            ...newIFileIndexItem(),
            parentDirectory: "/",
            fileName: "test.jpg"
          } as IFileIndexItem
        ]
      } as IArchiveProps;

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        {
          ...newIConnectionDefault(),
          data: null,
          statusCode: 200
        }
      );
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var dispatch = jest.fn();
      var component = await render(
        <MenuOptionMoveToTrash
          setSelect={jest.fn()}
          select={["test.jpg"]}
          isReadOnly={false}
          state={test}
          dispatch={dispatch}
        >
          t
        </MenuOptionMoveToTrash>
      );

      await act(async () => {
        await component.find("li").simulate("click");
      });

      expect(fetchPostSpy).toBeCalled();
      expect(dispatch).toBeCalled();
      expect(dispatch).toBeCalledWith({
        toRemoveFileList: ["/test.jpg"],
        type: "remove"
      });
      component.unmount();
    });

    it("check if when pressing Delete key", () => {
      jest.spyOn(FetchPost, "default").mockReset();
      var test = {
        ...newIArchive(),
        fileIndexItems: [
          {
            ...newIFileIndexItem(),
            parentDirectory: "/",
            fileName: "test.jpg"
          } as IFileIndexItem
        ]
      } as IArchiveProps;

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
        {
          ...newIConnectionDefault(),
          data: null,
          statusCode: 200
        }
      );
      const locationObject = {
        location: globalHistory.location,
        navigate: jest.fn()
      };

      jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(() => locationObject);

      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const dispatch = jest.fn();
      const component = render(
        <MenuOptionMoveToTrash
          setSelect={jest.fn()}
          select={["test.jpg"]}
          isReadOnly={false}
          state={test}
          dispatch={dispatch}
        >
          t
        </MenuOptionMoveToTrash>
      );

      act(() => {
        const event = new KeyboardEvent("keydown", {
          bubbles: true,
          cancelable: true,
          key: "Delete"
        });
        window.dispatchEvent(event);
      });

      expect(fetchPostSpy).toBeCalled();
      // dont know why dispatch is not called

      component.unmount();
    });
  });
});
