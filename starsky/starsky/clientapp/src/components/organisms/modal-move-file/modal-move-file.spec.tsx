import { render } from "@testing-library/react";
import React from "react";
import { act } from "react-dom/test-utils";
import * as useFileList from "../../../hooks/use-filelist";
import { IFileList } from "../../../hooks/use-filelist";
import * as useLocation from "../../../hooks/use-location";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Modal from "../../atoms/modal/modal";
import * as ItemTextListView from "../../molecules/item-text-list-view/item-text-list-view";
import ModalMoveFile from "./modal-move-file";

describe("ModalMoveFile", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalMoveFile
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
      ></ModalMoveFile>
    );
  });

  it("Input Not found", () => {
    // // use this import => import * as useFileList from '../hooks/use-filelist';
    jest.spyOn(useFileList, "default").mockImplementationOnce(() => {
      return null;
    });

    var modal = render(
      <ModalMoveFile
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
      ></ModalMoveFile>
    );

    expect(modal.queryByTestId("preloader-inside")).toBeTruthy();

    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    modal.unmount();
  });

  const startArchive = {
    archive: {
      fileIndexItems: [
        {
          filePath: "/test/",
          fileName: "test",
          isDirectory: true,
          status: IExifStatus.Ok
        },
        {
          filePath: "/image.jpg",
          fileName: "image.jpg",
          status: IExifStatus.ServerError,
          isDirectory: false
        }
      ]
    },
    pageType: PageType.Archive
  } as IFileList;

  const inTestFolderArchive = {
    archive: {
      fileIndexItems: [
        {
          filePath: "/test/photo.jpg",
          fileName: "photo.jpg",
          status: IExifStatus.Ok,
          isDirectory: false
        }
      ]
    },
    pageType: PageType.Archive
  } as IFileList;

  it("default disabled", () => {
    // detailview get archive parent item
    // use this import => import * as useFileList from '../hooks/use-filelist';
    jest.spyOn(useFileList, "default").mockImplementationOnce(() => {
      return startArchive;
    });

    var modal = render(
      <ModalMoveFile
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
      ></ModalMoveFile>
    );

    const btnTest = modal.queryByTestId("btn-test");
    expect(btnTest).toBeTruthy();

    const btnDefault = modal.queryByTestId(
      "modal-move-file-btn-default"
    ) as HTMLButtonElement;

    expect(btnDefault).toBeTruthy();

    // can't move to the same folder
    expect(btnDefault.disabled).toBeTruthy();

    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    modal.unmount();
  });

  it("go to parent folder", () => {
    jest.spyOn(FetchPost, "default").mockReset();

    // detailview get archive parent item
    // use this import => import * as useFileList from '../hooks/use-filelist';
    jest
      .spyOn(useFileList, "default")
      .mockImplementationOnce(() => {
        return startArchive;
      })
      .mockImplementationOnce(() => {
        return inTestFolderArchive;
      });

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: [
          {
            filePath: "/",
            status: IExifStatus.Ok
          }
        ]
      } as IConnectionDefault
    );
    var fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    var modal = render(
      <ModalMoveFile
        parentDirectory="/test"
        selectedSubPath="/test/test.jpg"
        isOpen={true}
        handleExit={() => {}}
      ></ModalMoveFile>
    );

    const parent = modal.queryByTestId("parent");

    act(() => {
      parent?.click();
    });

    const btnDefault = modal.queryByTestId(
      "modal-move-file-btn-default"
    ) as HTMLButtonElement;

    act(() => {
      // now move
      btnDefault?.click();
    });

    expect(fetchPostSpy).toBeCalledTimes(1);

    // generate url
    var bodyParams = new URLSearchParams();
    bodyParams.append("f", "/test/test.jpg");
    bodyParams.append("to", "/");
    bodyParams.append("collections", true.toString());

    expect(fetchPostSpy).toBeCalledWith(
      new UrlQuery().UrlSyncRename(),
      bodyParams.toString()
    );

    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    modal.unmount();
  });

  it("click to folder -> move", () => {
    // use this import => import * as useFileList from '../hooks/use-filelist';
    jest
      .spyOn(useFileList, "default")
      .mockImplementationOnce(() => startArchive)
      .mockImplementationOnce(() => inTestFolderArchive);

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    jest.spyOn(FetchPost, "default").mockReset();

    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        statusCode: 200,
        data: [
          {
            filePath: "test",
            status: IExifStatus.Ok,
            pageType: PageType.Archive
          }
        ]
      } as IConnectionDefault
    );
    var fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    var locationMockData = {
      location: jest.fn(),
      navigate: jest.fn()
    } as any;

    // use as ==> import * as useLocation from '../hooks/use-location';
    jest
      .spyOn(useLocation, "default")
      .mockImplementationOnce(() => locationMockData)
      .mockImplementationOnce(() => locationMockData)
      .mockImplementationOnce(() => locationMockData);

    var modal = render(
      <ModalMoveFile
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
      ></ModalMoveFile>
    );

    const btnTest = modal.queryByTestId("btn-test");
    expect(btnTest).toBeTruthy();

    act(() => {
      btnTest?.click();
    });

    const btnDefault = modal.queryByTestId(
      "modal-move-file-btn-default"
    ) as HTMLButtonElement;
    // button isn't disabled anymore
    expect(btnDefault.disabled).toBeFalsy();

    act(() => {
      // now move
      btnDefault?.click();
    });

    expect(fetchPostSpy).toBeCalledTimes(1);

    // generate url
    var bodyParams = new URLSearchParams();
    bodyParams.append("f", "/test.jpg");
    bodyParams.append("to", "/test/");
    bodyParams.append("collections", true.toString());

    expect(fetchPostSpy).toBeCalledWith(
      new UrlQuery().UrlSyncRename(),
      bodyParams.toString()
    );

    // and cleanup
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
      <ModalMoveFile
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={handleExitSpy}
      />
    );

    expect(handleExitSpy).toBeCalled();

    // and clean afterwards
    component.unmount();
  });

  describe("Fail situations", () => {
    beforeEach(() => {
      // spy on fetch
      // use this import => import * as FetchPost from '../shared/fetch-post';
      jest.spyOn(FetchPost, "default").mockReset();

      // use this import => import * as useFileList from '../hooks/use-filelist';
      jest
        .spyOn(useFileList, "default")
        .mockImplementationOnce(() => startArchive)
        .mockImplementationOnce(() => inTestFolderArchive)
        .mockImplementationOnce(() => inTestFolderArchive);
    });

    it("click to folder -> move and generic fail", () => {
      const mockIConnectionDefault = Promise.resolve({
        statusCode: 500,
        data: [
          {
            filePath: "test"
          }
        ]
      } as IConnectionDefault);

      jest
        .spyOn(FetchPost, "default")
        .mockImplementationOnce(() => mockIConnectionDefault)
        .mockImplementationOnce(() => mockIConnectionDefault);

      // import * as ItemTextListView from "../../molecules/item-text-list-view/item-text-list-view";
      jest
        .spyOn(ItemTextListView, "default")
        .mockImplementationOnce(() => null)
        .mockImplementationOnce(() => null);

      jest
        .spyOn(Modal, "default")
        .mockImplementationOnce((props) => <>{props.children}</>);

      var modal = render(
        <ModalMoveFile
          parentDirectory="/"
          selectedSubPath="/test.jpg"
          isOpen={true}
          handleExit={() => {}}
        >
          t
        </ModalMoveFile>
      );

      // Test is warning exist
      expect(modal.find(".warning-box")).toBeTruthy();

      // and cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      act(() => {
        modal.unmount();
      });
    });
  });
});
