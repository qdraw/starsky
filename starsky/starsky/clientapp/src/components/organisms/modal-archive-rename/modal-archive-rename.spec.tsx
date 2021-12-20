import { globalHistory } from "@reach/router";
import {
  createEvent,
  fireEvent,
  render,
  waitFor
} from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import * as useLocation from "../../../hooks/use-location";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Modal from "../../atoms/modal/modal";
import ModalArchiveRename from "./modal-archive-rename";

describe("ModalArchiveRename", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalArchiveRename isOpen={true} subPath="/" handleExit={() => {}}>
        test
      </ModalArchiveRename>
    );
  });
  describe("rename", () => {
    it("rename to non valid directory name", async () => {
      var modal = render(
        <ModalArchiveRename
          isOpen={true}
          subPath="/test"
          handleExit={() => {}}
        ></ModalArchiveRename>
      );

      const button = modal.queryByTestId(
        "modal-archive-rename-btn-default"
      ) as HTMLButtonElement;

      var submitButtonBefore = button.disabled;
      expect(submitButtonBefore).toBeTruthy();

      const directoryName = modal.queryByTestId(
        "form-control"
      ) as HTMLInputElement;

      // update component + now press a key
      act(() => {
        directoryName.textContent = "a";
        const inputEvent = createEvent.input(directoryName, { key: "a" });
        fireEvent(directoryName, inputEvent);
      });

      // await is needed => there is no button
      await act(async () => {
        await button.click();
      });

      expect(
        modal.queryByTestId("modal-archive-rename-warning-box")
      ).toBeTruthy();

      var submitButtonAfter = button.disabled;
      expect(submitButtonAfter).toBeTruthy();

      // Cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });

    it("change directory name", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../../../shared/fetch-post';;
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const handleExitSpy = jest.fn();
      const modal = render(
        <ModalArchiveRename
          isOpen={true}
          subPath="/test"
          handleExit={handleExitSpy}
        ></ModalArchiveRename>
      );

      const button = modal.queryByTestId(
        "modal-archive-rename-btn-default"
      ) as HTMLButtonElement;

      var submitButtonBefore = button.disabled;
      expect(submitButtonBefore).toBeTruthy();

      const directoryName = modal.queryByTestId(
        "form-control"
      ) as HTMLInputElement;

      // update component + now press a key
      act(() => {
        directoryName.textContent = "directory";
        const inputEvent = createEvent.input(directoryName, { key: "d" });
        fireEvent(directoryName, inputEvent);
      });

      expect(modal.queryByTestId("modal-archive-rename")).not.toBeNull();

      var submitButtonAfter = button.disabled;
      expect(submitButtonAfter).toBeFalsy();

      // await is needed => there is no button
      await act(async () => {
        await button.click();
      });

      await waitFor(() => expect(fetchPostSpy).toBeCalled());
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlDiskRename(),
        "f=%2Ftest&to=%2Fdirectory"
      );

      // Cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });

    it("change directory name and expect dispatch", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../../../shared/fetch-post';;
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const dispatch = jest.fn();
      const handleExitSpy = jest.fn();
      const modal = render(
        <ModalArchiveRename
          isOpen={true}
          subPath="/test"
          handleExit={handleExitSpy}
          dispatch={dispatch}
        ></ModalArchiveRename>
      );

      const button = modal.queryByTestId(
        "modal-archive-rename-btn-default"
      ) as HTMLButtonElement;

      const directoryName = modal.queryByTestId(
        "form-control"
      ) as HTMLInputElement;

      // update component + now press a key
      act(() => {
        directoryName.textContent = "directory";
        const inputEvent = createEvent.input(directoryName, { key: "d" });
        fireEvent(directoryName, inputEvent);
      });

      expect(modal.queryByTestId("modal-archive-rename")).not.toBeNull();

      var submitButtonAfter = button.disabled;
      expect(submitButtonAfter).toBeFalsy();

      // await is needed => there is no button
      await act(async () => {
        await button.click();
      });

      expect(dispatch).toBeCalled();
      expect(dispatch).toBeCalledWith({
        path: "/directory",
        type: "rename-folder"
      });

      // Cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });

    it("change directory name should give callback", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../../../shared/fetch-post';;
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const locationObject = {
        location: globalHistory.location,
        navigate: jest.fn()
      };

      jest
        .spyOn(useLocation, "default")
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject)
        .mockImplementationOnce(() => locationObject);

      const handleExitSpy = jest.fn();
      const modal = render(
        <ModalArchiveRename
          isOpen={true}
          subPath="/test"
          handleExit={handleExitSpy}
        ></ModalArchiveRename>
      );

      const button = modal.queryByTestId(
        "modal-archive-rename-btn-default"
      ) as HTMLButtonElement;

      const directoryName = modal.queryByTestId(
        "form-control"
      ) as HTMLInputElement;

      // update component + now press a key
      act(() => {
        directoryName.textContent = "directory";
        const inputEvent = createEvent.input(directoryName, { key: "d" });
        fireEvent(directoryName, inputEvent);
      });

      expect(modal.queryByTestId("modal-archive-rename")).not.toBeNull();

      var submitButtonAfter = button.disabled;
      expect(submitButtonAfter).toBeFalsy();

      // await is needed => there is no button
      await act(async () => {
        await button.click();
      });

      expect(handleExitSpy).toBeCalledWith("/directory");

      // Cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });

    it("change directory name and FAIL", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../../../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 500 } as IConnectionDefault);
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var modal = render(
        <ModalArchiveRename
          isOpen={true}
          subPath="/test"
          handleExit={() => {}}
        ></ModalArchiveRename>
      );

      const button = modal.queryByTestId(
        "modal-archive-rename-btn-default"
      ) as HTMLButtonElement;

      const directoryName = modal.queryByTestId(
        "form-control"
      ) as HTMLInputElement;

      // update component + now press a key
      act(() => {
        directoryName.textContent = "directory";
        const inputEvent = createEvent.input(directoryName, { key: "d" });
        fireEvent(directoryName, inputEvent);
      });

      expect(modal.queryByTestId("modal-archive-rename")).not.toBeNull();

      var submitButtonAfter = button.disabled;
      expect(submitButtonAfter).toBeFalsy();

      // await is needed => there is no button
      await act(async () => {
        await button.click();
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlDiskRename(),
        "f=%2Ftest&to=%2Fdirectory"
      );

      // Where should be a warning
      expect(
        modal.queryByTestId("modal-archive-rename-warning-box")
      ).toBeTruthy();

      // Cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });

    it("change directory name and FAIL with UNDO dispatch", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../../../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 500 } as IConnectionDefault);
      jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      const dispatch = jest.fn();
      var modal = render(
        <ModalArchiveRename
          isOpen={true}
          subPath="/test"
          handleExit={() => {}}
          dispatch={dispatch}
        ></ModalArchiveRename>
      );

      const button = modal.queryByTestId(
        "modal-archive-rename-btn-default"
      ) as HTMLButtonElement;

      const directoryName = modal.queryByTestId(
        "form-control"
      ) as HTMLInputElement;

      // update component + now press a key
      act(() => {
        directoryName.textContent = "directory";
        const inputEvent = createEvent.input(directoryName, { key: "d" });
        fireEvent(directoryName, inputEvent);
      });

      expect(modal.queryByTestId("modal-archive-rename")).not.toBeNull();

      // await is needed => there is no button
      await act(async () => {
        await button.click();
      });

      expect(dispatch).toBeCalled();

      expect(dispatch).toHaveBeenNthCalledWith(1, {
        path: "/directory",
        type: "rename-folder"
      });
      expect(dispatch).toHaveBeenNthCalledWith(2, {
        path: "/test",
        type: "rename-folder"
      });

      // Cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });

    it("test if handleExit is called", () => {
      // simulate if a user press on close
      // use as ==> import * as Modal from '../../atoms/modal/modal';
      jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>;
      });

      var handleExitSpy = jest.fn();

      var component = render(
        <ModalArchiveRename
          subPath="/"
          isOpen={true}
          handleExit={handleExitSpy}
        />
      );

      expect(handleExitSpy).toBeCalled();

      // and clean afterwards
      component.unmount();
    });
  });
});
