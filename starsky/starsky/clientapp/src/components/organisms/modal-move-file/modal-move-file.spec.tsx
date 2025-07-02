import { render, screen, waitFor } from "@testing-library/react";
import { act } from "react";
import * as useFileList from "../../../hooks/use-filelist";
import { IFileList } from "../../../hooks/use-filelist";
import { IUseLocation } from "../../../hooks/use-location/interfaces/IUseLocation";
import * as useLocation from "../../../hooks/use-location/use-location";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { PageType } from "../../../interfaces/IDetailView";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import { UrlQuery } from "../../../shared/url/url-query";
import * as Modal from "../../atoms/modal/modal";
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

    const modal = render(
      <ModalMoveFile
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
      ></ModalMoveFile>
    );

    expect(screen.getByTestId("preloader-inside")).toBeTruthy();

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

    const modal = render(
      <ModalMoveFile
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
      ></ModalMoveFile>
    );

    const btnTest = screen.queryByTestId("btn-test");
    expect(btnTest).toBeTruthy();

    const btnDefault = screen.queryByTestId("modal-move-file-btn-default") as HTMLButtonElement;

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
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: [
        {
          filePath: "/",
          status: IExifStatus.Ok
        }
      ]
    } as IConnectionDefault);
    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const modal = render(
      <ModalMoveFile
        parentDirectory="/test"
        selectedSubPath="/test/test.jpg"
        isOpen={true}
        handleExit={() => {}}
      ></ModalMoveFile>
    );

    const parent = screen.queryByTestId("parent");

    act(() => {
      parent?.click();
    });

    const btnDefault = screen.queryByTestId("modal-move-file-btn-default") as HTMLButtonElement;

    act(() => {
      // now move
      btnDefault?.click();
    });

    expect(fetchPostSpy).toHaveBeenCalledTimes(1);

    // generate url
    const bodyParams = new URLSearchParams();
    bodyParams.append("f", "/test/test.jpg");
    bodyParams.append("to", "/");
    bodyParams.append("collections", true.toString());

    expect(fetchPostSpy).toHaveBeenCalledWith(
      new UrlQuery().UrlDiskRename(),
      bodyParams.toString()
    );

    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    modal.unmount();
  });

  it("multi file click to folder -> move ", () => {
    // use this import => import * as useFileList from '../hooks/use-filelist';
    jest
      .spyOn(useFileList, "default")
      .mockImplementationOnce(() => startArchive)
      .mockImplementationOnce(() => inTestFolderArchive);

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    jest.spyOn(FetchPost, "default").mockReset();

    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: [
        {
          filePath: "test",
          status: IExifStatus.Ok,
          pageType: PageType.Archive
        }
      ]
    } as IConnectionDefault);
    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const locationMockData = {
      location: jest.fn(),
      navigate: jest.fn()
    } as unknown as IUseLocation;

    // use as ==> import * as useLocation from '../hooks/use-location/use-location';
    jest
      .spyOn(useLocation, "default")
      .mockReset()
      .mockImplementationOnce(() => locationMockData)
      .mockImplementationOnce(() => locationMockData)
      .mockImplementationOnce(() => locationMockData);

    const modal = render(
      <ModalMoveFile
        parentDirectory="/"
        selectedSubPath="/test.jpg;/test2.jpg"
        isOpen={true}
        handleExit={() => {}}
      ></ModalMoveFile>
    );

    const btnTest = screen.queryByTestId("btn-test");
    expect(btnTest).toBeTruthy();

    act(() => {
      btnTest?.click();
    });

    const btnDefault = screen.queryByTestId("modal-move-file-btn-default") as HTMLButtonElement;
    // button isn't disabled anymore
    expect(btnDefault.disabled).toBeFalsy();

    act(() => {
      // now move
      btnDefault?.click();
    });

    expect(fetchPostSpy).toHaveBeenCalledTimes(1);

    // generate url
    const bodyParams = new URLSearchParams();
    bodyParams.append("f", "/test.jpg;/test2.jpg");
    bodyParams.append("to", "/test/;/test/");
    bodyParams.append("collections", true.toString());

    expect(fetchPostSpy).toHaveBeenCalledWith(
      new UrlQuery().UrlDiskRename(),
      bodyParams.toString()
    );

    // and cleanup
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
    modal.unmount();
  });

  it("single file click to folder -> move ", () => {
    // use this import => import * as useFileList from '../hooks/use-filelist';
    jest
      .spyOn(useFileList, "default")
      .mockImplementationOnce(() => startArchive)
      .mockImplementationOnce(() => inTestFolderArchive);

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    jest.spyOn(FetchPost, "default").mockReset();

    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
      statusCode: 200,
      data: [
        {
          filePath: "test",
          status: IExifStatus.Ok,
          pageType: PageType.Archive
        }
      ]
    } as IConnectionDefault);
    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const locationMockData = {
      location: jest.fn(),
      navigate: jest.fn()
    } as unknown as IUseLocation;

    // use as ==> import * as useLocation from '../hooks/use-location/use-location';
    jest
      .spyOn(useLocation, "default")
      .mockImplementationOnce(() => locationMockData)
      .mockImplementationOnce(() => locationMockData)
      .mockImplementationOnce(() => locationMockData);

    const modal = render(
      <ModalMoveFile
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
      ></ModalMoveFile>
    );

    const btnTest = screen.queryByTestId("btn-test");
    expect(btnTest).toBeTruthy();

    act(() => {
      btnTest?.click();
    });

    const btnDefault = screen.queryByTestId("modal-move-file-btn-default") as HTMLButtonElement;
    // button isn't disabled anymore
    expect(btnDefault.disabled).toBeFalsy();

    act(() => {
      // now move
      btnDefault?.click();
    });

    expect(fetchPostSpy).toHaveBeenCalledTimes(1);

    // generate url
    const bodyParams = new URLSearchParams();
    bodyParams.append("f", "/test.jpg");
    bodyParams.append("to", "/test/");
    bodyParams.append("collections", true.toString());

    expect(fetchPostSpy).toHaveBeenCalledWith(
      new UrlQuery().UrlDiskRename(),
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

    const handleExitSpy = jest.fn();

    const component = render(
      <ModalMoveFile
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={handleExitSpy}
      />
    );

    expect(handleExitSpy).toHaveBeenCalled();

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
        .mockReset()
        .mockImplementationOnce(() => startArchive)
        .mockImplementationOnce(() => inTestFolderArchive)
        .mockImplementationOnce(() => inTestFolderArchive);
    });

    it("click to folder -> move and generic fail", async () => {
      const mockIConnectionDefault = Promise.resolve({
        statusCode: 500,
        data: [
          {
            filePath: "test"
          }
        ]
      } as IConnectionDefault);

      const fetchPostSpy = jest
        .spyOn(FetchPost, "default")
        .mockReset()
        .mockImplementationOnce(() => mockIConnectionDefault)
        .mockImplementationOnce(() => mockIConnectionDefault);

      jest
        .spyOn(Modal, "default")
        .mockReset()
        .mockImplementationOnce((props) => <>{props.children}</>)
        .mockImplementationOnce((props) => <>{props.children}</>)
        .mockImplementationOnce((props) => <>{props.children}</>);

      const modal = render(
        <ModalMoveFile
          parentDirectory="/"
          selectedSubPath="/test.jpg"
          isOpen={true}
          handleExit={() => {}}
        ></ModalMoveFile>
      );
      const btnTest = screen.queryByTestId("btn-test");
      expect(btnTest).toBeTruthy();

      await act(async () => {
        await btnTest?.click();
      });

      const btnDefault = screen.queryByTestId("modal-move-file-btn-default") as HTMLButtonElement;
      // button isn't disabled anymore
      expect(btnDefault.disabled).toBeFalsy();

      await act(async () => {
        // now move
        await btnDefault?.click();
      });

      await waitFor(() => expect(fetchPostSpy).toHaveBeenCalled());

      // Test is warning exist
      expect(screen.getByTestId("modal-move-file-warning-box")).toBeTruthy();

      // and cleanup
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
      act(() => {
        modal.unmount();
      });
    });
  });
});
