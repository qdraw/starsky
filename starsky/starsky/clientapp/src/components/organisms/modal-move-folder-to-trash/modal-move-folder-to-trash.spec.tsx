import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { act } from "react-dom/test-utils";
import * as IUseLocation from "../../../hooks/use-location";
import {
  IConnectionDefault,
  newIConnectionDefault
} from "../../../interfaces/IConnectionDefault";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchPost from "../../../shared/fetch-post";
import { UrlQuery } from "../../../shared/url-query";
import * as Modal from "../../atoms/modal/modal";
import ModalMoveFolderToTrash from "./modal-move-folder-to-trash";

describe("ModalMoveFolderToTrash component", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("should render the modal when `isOpen` is true", () => {
    const handleExitMock = jest.fn();
    const setIsLoadingMock = jest.fn();

    const modalSpy = jest
      .spyOn(Modal, "default")
      .mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>;
      });

    render(
      <ModalMoveFolderToTrash
        isOpen={true}
        subPath={"/path/to/folder"}
        handleExit={handleExitMock}
        setIsLoading={setIsLoadingMock}
      />
    );
    expect(modalSpy).toBeCalledTimes(1);
  });

  it("should call `handleExit` when cancel button is clicked", async () => {
    const handleExitMock = jest.fn();
    const setIsLoadingMock = jest.fn();

    jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
      props.handleExit();
      return <>{props.children}</>;
    });

    render(
      <ModalMoveFolderToTrash
        isOpen={true}
        subPath={"/path/to/folder"}
        handleExit={handleExitMock}
        setIsLoading={setIsLoadingMock}
      />
    );
    const cancelButton = screen.getByTestId("force-cancel");
    fireEvent.click(cancelButton);
    await waitFor(() => {
      expect(handleExitMock).toHaveBeenCalled();
    });
  });

  it("should call `handleExit` and `setIsLoading` when Move to Trash button is clicked", async () => {
    const handleExitMock = jest.fn();
    const setIsLoadingMock = jest.fn();

    jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
      props.handleExit();
      return <>{props.children}</>;
    });

    render(
      <ModalMoveFolderToTrash
        isOpen={true}
        subPath={"/path/to/folder"}
        handleExit={handleExitMock}
        setIsLoading={setIsLoadingMock}
      />
    );
    const moveButton = screen.getByTestId("move-folder-to-trash");
    fireEvent.click(moveButton);
    await waitFor(() => {
      expect(setIsLoadingMock).toHaveBeenCalledWith(true);
    });
    await waitFor(() => {
      expect(handleExitMock).toHaveBeenCalled();
    });
  });

  it("calls FetchPost function with the expected parameters when moveFolderIntoTrash function is called", () => {
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        ...newIConnectionDefault(),
        data: [
          {
            status: IExifStatus.Ok,
            fileName: "rootfilename.jpg",
            fileIndexItem: {
              description: "",
              fileHash: undefined,
              fileName: "test.jpg",
              filePath: "/test.jpg",
              isDirectory: false,
              status: "Ok",
              tags: "",
              title: ""
            }
          }
        ],
        statusCode: 200
      }
    );
    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const subPath = "path/to/folder";
    const setIsLoading = jest.fn();
    const handleExit = jest.fn();

    jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
      props.handleExit();
      return <>{props.children}</>;
    });

    // Arrange
    const props = {
      isOpen: true,
      subPath: subPath,
      handleExit: handleExit,
      setIsLoading: setIsLoading
    };
    render(<ModalMoveFolderToTrash {...props} />);
    const button = screen.getByTestId("move-folder-to-trash");

    // Act
    fireEvent.click(button);

    // Assert
    const expectedUrl = new UrlQuery().UrlMoveToTrashApi();
    const expectedBodyParams = new URLSearchParams();
    expectedBodyParams.append("f", subPath);
    expect(fetchPostSpy).toHaveBeenCalledWith(
      expectedUrl,
      expectedBodyParams.toString(),
      "post"
    );
  });

  it("calls FetchPost with 400 result with the expected parameters when moveFolderIntoTrash function is called", () => {
    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        ...newIConnectionDefault(),
        data: [
          {
            status: IExifStatus.Ok,
            fileName: "rootfilename.jpg",
            fileIndexItem: {
              description: "",
              fileHash: undefined,
              fileName: "test.jpg",
              filePath: "/test.jpg",
              isDirectory: false,
              status: "Ok",
              tags: "",
              title: ""
            }
          }
        ],
        statusCode: 400
      }
    );
    const fetchPostSpy = jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    const subPath = "path/to/folder";
    const setIsLoading = jest.fn();
    const handleExit = jest.fn();

    jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
      props.handleExit();
      return <>{props.children}</>;
    });

    // Arrange
    const props = {
      isOpen: true,
      subPath: subPath,
      handleExit: handleExit,
      setIsLoading: setIsLoading
    };
    render(<ModalMoveFolderToTrash {...props} />);
    const button = screen.getByTestId("move-folder-to-trash");

    // Act
    fireEvent.click(button);

    // Assert
    const expectedUrl = new UrlQuery().UrlMoveToTrashApi();
    const expectedBodyParams = new URLSearchParams();
    expectedBodyParams.append("f", subPath);
    expect(fetchPostSpy).toHaveBeenCalledWith(
      expectedUrl,
      expectedBodyParams.toString(),
      "post"
    );
  });

  it("should navigate to parent folder after moving folder to trash", async () => {
    const subPath = "/path/to/folder";
    const setIsLoading = jest.fn();
    const handleExit = jest.fn();

    // spy on fetch
    // use this import => import * as FetchPost from '../shared/fetch-post';
    const mockIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve(
      {
        ...newIConnectionDefault(),
        data: [
          {
            status: IExifStatus.Ok,
            fileName: "rootfilename.jpg",
            fileIndexItem: {
              description: "",
              fileHash: undefined,
              fileName: "test.jpg",
              filePath: "/test.jpg",
              isDirectory: false,
              status: "Ok",
              tags: "",
              title: ""
            }
          }
        ],
        statusCode: 200
      }
    );
    jest
      .spyOn(FetchPost, "default")
      .mockImplementationOnce(() => mockIConnectionDefault);

    await act(async () => {
      jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>;
      });
    });

    // create a mock history object with a location object
    const historySpy = jest
      .spyOn(IUseLocation, "default")
      .mockImplementationOnce(() => {
        return {
          location: {
            pathname: "/",
            search: "?path=/path/to/folder"
          },
          navigate: jest.fn()
        } as any;
      });

    // render the component and pass in the mock history object
    render(
      <ModalMoveFolderToTrash
        isOpen={true}
        subPath={subPath}
        handleExit={handleExit}
        setIsLoading={setIsLoading}
      />
    );

    // click the Move to Trash button
    const moveButton = screen.getByTestId("move-folder-to-trash");
    fireEvent.click(moveButton);

    // wait for the async FetchPost to complete
    await waitFor(() => {
      expect(setIsLoading).toHaveBeenCalledTimes(1);
    });

    // wait for the history.navigate to be called with the updated location
    await waitFor(() => {
      expect(historySpy).toBeCalled();
    });
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
      <ModalMoveFolderToTrash
        isOpen={true}
        subPath={"subPath"}
        handleExit={handleExitSpy}
        setIsLoading={jest.fn()}
      />
    );

    expect(handleExitSpy).toBeCalled();

    component.unmount();
  });
});
