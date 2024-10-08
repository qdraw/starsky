import { fireEvent, render, screen } from "@testing-library/react";
import { act } from "react";
import * as useLocation from "../../../hooks/use-location/use-location";
import { newIArchive } from "../../../interfaces/IArchive";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IConnectionDefault, newIConnectionDefault } from "../../../interfaces/IConnectionDefault";
import {
  IFileIndexItem,
  newIFileIndexItem,
  newIFileIndexItemArray
} from "../../../interfaces/IFileIndexItem";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import MenuOptionMoveToTrash from "./menu-option-move-to-trash";

describe("MenuOptionMoveToTrash", () => {
  it("renders", () => {
    const test = {
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
    it("check if dispatch when click", async () => {
      jest.spyOn(FetchPost, "default").mockReset();
      const test = {
        ...newIArchive(),
        fileIndexItems: [
          {
            ...newIFileIndexItem(),
            parentDirectory: "/",
            fileName: "test.jpg"
          } as IFileIndexItem
        ]
      } as IArchiveProps;

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: null,
        statusCode: 200
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const dispatch = jest.fn();
      const component = await render(
        <MenuOptionMoveToTrash
          setSelect={jest.fn()}
          select={["test.jpg"]}
          isReadOnly={false}
          state={test}
          dispatch={dispatch}
        />
      );

      const trashButton = screen.queryByTestId("trash") as HTMLButtonElement;
      expect(trashButton).toBeTruthy();

      await act(async () => {
        await trashButton.click();
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(dispatch).toHaveBeenCalled();
      expect(dispatch).toHaveBeenCalledWith({
        toRemoveFileList: ["/test.jpg"],
        type: "remove"
      });
      component.unmount();
    });

    it("check if dispatch when keyDown enter", async () => {
      jest.spyOn(FetchPost, "default").mockReset();
      const test = {
        ...newIArchive(),
        fileIndexItems: [
          {
            ...newIFileIndexItem(),
            parentDirectory: "/",
            fileName: "test.jpg"
          } as IFileIndexItem
        ]
      } as IArchiveProps;

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: null,
        statusCode: 200
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const dispatch = jest.fn();
      const component = await render(
        <MenuOptionMoveToTrash
          setSelect={jest.fn()}
          select={["test.jpg"]}
          isReadOnly={false}
          state={test}
          dispatch={dispatch}
        />
      );

      const trashButton = screen.queryByTestId("trash") as HTMLButtonElement;
      expect(trashButton).toBeTruthy();

      await act(async () => {
        await fireEvent.keyDown(trashButton, {
          key: "Enter"
        });
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(dispatch).toHaveBeenCalled();
      expect(dispatch).toHaveBeenCalledWith({
        toRemoveFileList: ["/test.jpg"],
        type: "remove"
      });
      component.unmount();
    });

    it("check if dispatch when keyDown tab so skip", async () => {
      jest.spyOn(FetchPost, "default").mockReset();
      const test = {
        ...newIArchive(),
        fileIndexItems: [
          {
            ...newIFileIndexItem(),
            parentDirectory: "/",
            fileName: "test.jpg"
          } as IFileIndexItem
        ]
      } as IArchiveProps;

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: null,
        statusCode: 200
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const dispatch = jest.fn();
      const component = await render(
        <MenuOptionMoveToTrash
          setSelect={jest.fn()}
          select={["test.jpg"]}
          isReadOnly={false}
          state={test}
          dispatch={dispatch}
        />
      );

      const trashButton = screen.queryByTestId("trash") as HTMLButtonElement;
      expect(trashButton).toBeTruthy();

      await act(async () => {
        await fireEvent.keyDown(trashButton, {
          key: "Tab"
        });
      });

      expect(fetchPostSpy).toHaveBeenCalledTimes(0);
      expect(dispatch).toHaveBeenCalledTimes(0);

      component.unmount();
    });

    it("check if not dispatch when error", () => {
      console.log("-- check if not dispatch when error");

      jest.spyOn(FetchPost, "default").mockReset();
      const test = {
        ...newIArchive(),
        fileIndexItems: [
          {
            ...newIFileIndexItem(),
            parentDirectory: "/",
            fileName: "test.jpg"
          } as IFileIndexItem
        ]
      } as IArchiveProps;

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: null,
        statusCode: 400 // -- error
      });
      const fetchPostSpy = jest
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
        />
      );

      const trashButton = screen.queryByTestId("trash") as HTMLButtonElement;
      expect(trashButton).toBeTruthy();

      act(() => {
        trashButton.click();
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      expect(dispatch).toHaveBeenCalledTimes(0);
      component.unmount();
    });

    it("check if not dispatch when readonly", () => {
      console.log("-- check if not dispatch when readonly");

      jest.spyOn(FetchPost, "default").mockReset();
      const test = {
        ...newIArchive(),
        fileIndexItems: [
          {
            ...newIFileIndexItem(),
            parentDirectory: "/",
            fileName: "test.jpg"
          } as IFileIndexItem
        ]
      } as IArchiveProps;

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: null,
        statusCode: 400 // -- error
      });
      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const dispatch = jest.fn();
      const component = render(
        <MenuOptionMoveToTrash
          setSelect={jest.fn()}
          select={["test.jpg"]}
          isReadOnly={true}
          state={test}
          dispatch={dispatch}
        />
      );

      const trashButton = screen.queryByTestId("trash") as HTMLButtonElement;
      expect(trashButton).toBeTruthy();

      act(() => {
        trashButton.click();
      });

      expect(fetchPostSpy).toHaveBeenCalledTimes(0);
      expect(dispatch).toHaveBeenCalledTimes(0);
      component.unmount();
    });
  });

  describe("context 2", () => {
    it("check if when pressing Delete key", () => {
      console.log("-- check if when pressing Delete key");

      jest.spyOn(FetchPost, "default").mockClear();
      const test = {
        ...newIArchive(),
        fileIndexItems: [
          {
            ...newIFileIndexItem(),
            parentDirectory: "/",
            fileName: "test.jpg"
          } as IFileIndexItem
        ]
      } as IArchiveProps;

      const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        ...newIConnectionDefault(),
        data: null,
        statusCode: 200
      });
      const locationObject = {
        location: window.location,
        navigate: jest.fn()
      };

      jest.spyOn(useLocation, "default").mockImplementationOnce(() => locationObject);

      const fetchPostSpy = jest
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
        />
      );

      act(() => {
        const event = new KeyboardEvent("keydown", {
          bubbles: true,
          cancelable: true,
          key: "Delete"
        });
        window.dispatchEvent(event);
      });

      expect(fetchPostSpy).toHaveBeenCalled();
      // don't know why dispatch is not called

      component.unmount();
    });
  });
});
