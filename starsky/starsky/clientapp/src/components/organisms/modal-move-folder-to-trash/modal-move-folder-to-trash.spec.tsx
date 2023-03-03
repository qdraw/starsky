import { fireEvent, render, screen, waitFor } from "@testing-library/react";
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
    const moveButton = screen.getByTestId("force-delete");
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
        ]
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
    const button = screen.getByTestId("force-delete");

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
});
