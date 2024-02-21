import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import ModalDesktopEditorOpenSelectionConfirmation from "./modal-desktop-editor-open-selection-confirmation";

describe("ModalDesktopEditorOpenConfirmation", () => {
  afterEach(() => {
    jest.restoreAllMocks();
  });

  const exampleState = {
    fileIndexItems: [
      {
        fileName: "test.jpg",
        parentDirectory: "/"
      }
    ]
  } as IArchiveProps;

  it("renders correctly", () => {
    const component = render(
      <ModalDesktopEditorOpenSelectionConfirmation
        isOpen={true}
        select={[]}
        handleExit={() => {}}
        state={exampleState}
        setIsLoading={() => {}}
        isCollections={false}
      />
    );

    expect(screen.getByTestId("editor-open-heading")).toBeTruthy();
    expect(screen.getByTestId("editor-open-text")).toBeTruthy();
    expect(screen.getByTestId("editor-open-confirmation-no")).toBeTruthy();
    expect(screen.getByTestId("editor-open-confirmation-yes")).toBeTruthy();

    component.unmount();
  });

  it("calls handleExit on cancel button click", () => {
    const handleExit = jest.fn();
    const component = render(
      <ModalDesktopEditorOpenSelectionConfirmation
        isOpen={true}
        handleExit={handleExit}
        state={exampleState}
        select={[]}
        setIsLoading={() => {}}
        isCollections={false}
      />
    );

    fireEvent.click(screen.getByTestId("editor-open-confirmation-no"));
    expect(handleExit).toHaveBeenCalled();

    component.unmount();
  });

  it("calls OpenDesktop and handleExit on confirm button click", async () => {
    const handleExit = jest.fn();
    const setIsLoading = jest.fn();

    const mockIConnectionDefaultResolve: Promise<IConnectionDefault> = Promise.resolve({
      data: false,
      statusCode: 200
    });
    const mockFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockImplementation(() => mockIConnectionDefaultResolve);

    const component = render(
      <ModalDesktopEditorOpenSelectionConfirmation
        isOpen={true}
        handleExit={handleExit}
        state={exampleState}
        select={[]}
        setIsLoading={setIsLoading}
        isCollections={false}
      />
    );

    fireEvent.click(screen.getByTestId("editor-open-confirmation-yes"));

    await waitFor(() => {
      expect(mockFetchPost).toHaveBeenCalled();
      expect(handleExit).toHaveBeenCalled();

      component.unmount();
    });
  });

  it("calls setIsError if FetchPost fails", async () => {
    const mockIConnectionDefaultResolve = Promise.resolve({
      data: false,
      statusCode: 400
    });

    const mockFetchPost = jest
      .spyOn(FetchPost, "default")
      .mockReset()
      .mockImplementation(() => mockIConnectionDefaultResolve)
      .mockImplementation(() => mockIConnectionDefaultResolve);

    const component = render(
      <ModalDesktopEditorOpenSelectionConfirmation
        isOpen={true}
        select={["test.jpg"]}
        handleExit={() => {}}
        state={exampleState}
        setIsLoading={() => {}}
        isCollections={false}
      />
    );

    fireEvent.click(screen.getByTestId("editor-open-confirmation-yes"));

    await waitFor(() => {
      expect(mockFetchPost).toHaveBeenCalled();
      expect(screen.getByTestId("editor-open-error")).toBeTruthy();

      component.unmount();
    });
  });
});
