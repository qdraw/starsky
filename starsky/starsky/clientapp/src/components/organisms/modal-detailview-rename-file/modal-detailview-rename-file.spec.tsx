import { act, createEvent, fireEvent, render } from "@testing-library/react";
import React from "react";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IDetailView } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Modal from "../../atoms/modal/modal";
import ModalDetailviewRenameFile from "./modal-detailview-rename-file";

describe("ModalDetailviewRenameFile", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalDetailviewRenameFile
        state={{} as any}
        isOpen={true}
        handleExit={() => {}}
      >
        test
      </ModalDetailviewRenameFile>
    );
  });

  describe("rename", () => {
    it("to wrong extension", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var state = {
        fileIndexItem: {
          status: IExifStatus.Ok,
          filePath: "/test/image.jpg",
          fileName: "image.jpg"
        }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      var modal = render(
        <ModalDetailviewRenameFile
          isOpen={true}
          state={state}
          handleExit={() => {}}
        ></ModalDetailviewRenameFile>
      );

      const button = modal.queryByTestId(
        "modal-detailview-rename-file-btn-default"
      ) as HTMLButtonElement;

      var submitButtonBefore = button.disabled;
      expect(submitButtonBefore).toBeTruthy();

      const directoryName = modal.queryByTestId(
        "form-control"
      ) as HTMLInputElement;

      // update component + now press a key
      act(() => {
        directoryName.textContent = "file-with-different-extension.tiff";
        const inputEvent = createEvent.input(directoryName, { key: "a" });
        fireEvent(directoryName, inputEvent);
      });

      // await is needed => there is no button
      await act(async () => {
        await button.click();
      });

      expect(
        modal.queryByTestId("modal-detailview-rename-file-warning-box")
      ).toBeTruthy();

      var submitButtonAfter = button.disabled;
      expect(submitButtonAfter).toBeTruthy();

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlSyncRename(),
        "f=%2Ftest%2Fimage.jpg&to=%2Ftest%2Ffile-with-different-extension.tiff&collections=true"
      );

      // cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });

    it("to non valid extension", async () => {
      var state = {
        fileIndexItem: {
          status: IExifStatus.Ok,
          filePath: "/test/image.jpg",
          fileName: "image.jpg"
        }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      var modal = render(
        <ModalDetailviewRenameFile
          isOpen={true}
          state={state}
          handleExit={() => {}}
        ></ModalDetailviewRenameFile>
      );

      const button = modal.queryByTestId(
        "modal-detailview-rename-file-btn-default"
      ) as HTMLButtonElement;

      var submitButtonBefore = button.disabled;
      expect(submitButtonBefore).toBeTruthy();

      const directoryName = modal.queryByTestId(
        "form-control"
      ) as HTMLInputElement;

      // update component + now press a key
      act(() => {
        directoryName.textContent = "file-without-extension";
        const inputEvent = createEvent.input(directoryName, { key: "a" });
        fireEvent(directoryName, inputEvent);
      });

      // await is needed => there is no button
      await act(async () => {
        await button.click();
      });

      expect(
        modal.queryByTestId("modal-detailview-rename-file-warning-box")
      ).toBeTruthy();

      var submitButtonAfter = button.disabled;
      expect(submitButtonAfter).toBeTruthy();

      // cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });

    it("submit filename change", async () => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      const mockIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({ statusCode: 200 } as IConnectionDefault);
      var fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault);

      var state = {
        fileIndexItem: {
          status: IExifStatus.Ok,
          filePath: "/test/image.jpg",
          fileName: "image.jpg"
        }
      } as IDetailView;
      var contextValues = { state, dispatch: jest.fn() };

      jest
        .spyOn(React, "useContext")
        .mockImplementationOnce(() => {
          return contextValues;
        })
        .mockImplementationOnce(() => {
          return contextValues;
        });

      var modal = render(
        <ModalDetailviewRenameFile
          isOpen={true}
          state={state}
          handleExit={() => {}}
        ></ModalDetailviewRenameFile>
      );

      const button = modal.queryByTestId(
        "modal-detailview-rename-file-btn-default"
      ) as HTMLButtonElement;

      const directoryName = modal.queryByTestId(
        "form-control"
      ) as HTMLInputElement;

      // update component + now press a key
      act(() => {
        directoryName.textContent = "name.jpg";
        const inputEvent = createEvent.input(directoryName, { key: "a" });
        fireEvent(directoryName, inputEvent);
      });

      // await is needed => there is no button
      await act(async () => {
        await button.click();
      });

      expect(fetchPostSpy).toBeCalled();
      expect(fetchPostSpy).toBeCalledWith(
        new UrlQuery().UrlSyncRename(),
        "f=%2Ftest%2Fimage.jpg&to=%2Ftest%2Fname.jpg&collections=true"
      );

      // cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      modal.unmount();
    });

    it("test if handleExit is called", () => {
      // simulate if a user press on close
      // use as ==> import * as Modal from './modal';
      jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>;
      });

      var handleExitSpy = jest.fn();

      var component = render(
        <ModalDetailviewRenameFile
          state={{} as any}
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
