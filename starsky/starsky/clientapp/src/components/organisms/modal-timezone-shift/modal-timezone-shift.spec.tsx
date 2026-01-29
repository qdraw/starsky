import { fireEvent, render, screen } from "@testing-library/react";
import { act } from "react";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import * as FetchGet from "../../../shared/fetch/fetch-get";
import * as FetchPost from "../../../shared/fetch/fetch-post";
import ModalTimezoneShift from "./modal-timezone-shift";

describe("ModalTimezoneShift", () => {
  const mockState = {
    fileIndexItems: [
      {
        fileName: "test.jpg",
        filePath: "/test.jpg",
        dateTime: "2024-08-12T14:32:00",
        colorClass: 0,
        fileCollectionName: "test",
        fileHash: "abc123",
        parentDirectory: "/",
        status: IExifStatus.Default
      }
    ],
    relativeObjects: {},
    subPath: "",
    breadcrumb: [],
    colorClassActiveList: [],
    colorClassUsage: [],
    collectionsCount: 0,
    pageType: "Archive",
    isReadOnly: false,
    dateCache: 0
  } as unknown as IArchiveProps;
  const mockDispatch = jest.fn();
  const mockUndoSelection = jest.fn();
  const mockHistoryLocationSearch = "";

  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementation(() => {});
    mockDispatch.mockClear();
    mockUndoSelection.mockClear();
  });

  it("renders when open", () => {
    render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={() => {}}
        select={["/test.jpg"]}
        historyLocationSearch={mockHistoryLocationSearch}
        state={mockState}
        dispatch={mockDispatch}
        undoSelection={mockUndoSelection}
      />
    );
  });

  it("shows mode selection initially", () => {
    const component = render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={() => {}}
        select={["/test.jpg"]}
        historyLocationSearch={mockHistoryLocationSearch}
        state={mockState}
        dispatch={mockDispatch}
        undoSelection={mockUndoSelection}
      />
    );

    expect(screen.getByText(/Shift Photo Timestamps/i)).toBeTruthy();
    expect(screen.getByText(/What do you want to do/i)).toBeTruthy();
    component.unmount();
  });

  it("displays correct file count", () => {
    const component = render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={() => {}}
        select={["/test1.jpg", "/test2.jpg", "/test3.jpg"]}
        historyLocationSearch={mockHistoryLocationSearch}
        state={mockState}
        dispatch={mockDispatch}
        undoSelection={mockUndoSelection}
      />
    );

    expect(screen.getByText(/You have selected 3 images/i)).toBeTruthy();
    component.unmount();
  });

  it("switches to offset mode when radio is selected", () => {
    const component = render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={() => {}}
        select={["/test.jpg"]}
        historyLocationSearch={mockHistoryLocationSearch}
        state={mockState}
        dispatch={mockDispatch}
        undoSelection={mockUndoSelection}
      />
    );

    const offsetRadio = screen.getByRole("radio", { name: /Correct incorrect camera timezone/i });
    fireEvent.click(offsetRadio);

    expect(screen.getByText(/Correct Camera Time/i)).toBeTruthy();
    expect(screen.getByText(/Time offsets/i)).toBeTruthy();
    component.unmount();
  });

  it("switches to timezone mode when radio is selected", async () => {
    const mockTimezones = [
      { id: "Europe/London", displayName: "(UTC+00:00) London" },
      { id: "Europe/Amsterdam", displayName: "(UTC+01:00) Amsterdam" }
    ];

    jest.spyOn(FetchGet, "default").mockResolvedValue({
      statusCode: 200,
      data: mockTimezones
    });

    const component = render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={() => {}}
        select={["/test.jpg"]}
        historyLocationSearch={mockHistoryLocationSearch}
        state={mockState}
        dispatch={mockDispatch}
        undoSelection={mockUndoSelection}
      />
    );

    const timezoneRadio = screen.getByRole("radio", { name: /I moved to a different place/i });

    await act(async () => {
      fireEvent.click(timezoneRadio);
    });

    expect(screen.getByText(/Change Location/i)).toBeTruthy();
    component.unmount();
  });

  it("generates preview for offset mode", async () => {
    const mockPreview = [
      {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T17:32:00",
        delta: "+03:00:00",
        warning: "",
        error: ""
      }
    ];

    jest.spyOn(FetchPost, "default").mockResolvedValue({
      statusCode: 200,
      data: mockPreview
    });

    const component = render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={() => {}}
        select={["/test.jpg"]}
        historyLocationSearch={mockHistoryLocationSearch}
        state={mockState}
        dispatch={mockDispatch}
        undoSelection={mockUndoSelection}
      />
    );

    const offsetRadio = screen.getByRole("radio", { name: /Correct incorrect camera timezone/i });
    fireEvent.click(offsetRadio);

    const hoursInput = screen.getByLabelText(/Hours/i);
    fireEvent.change(hoursInput, { target: { value: "3" } });

    // Wait for the preview to appear (since preview is generated automatically)
    await screen.findByText(/Result:/i);
    expect(FetchPost.default).toHaveBeenCalled();
    component.unmount();
  });

  it("calls handleExit when Cancel is clicked", () => {
    const mockHandleExit = jest.fn();
    const component = render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={mockHandleExit}
        select={["/test.jpg"]}
        historyLocationSearch={mockHistoryLocationSearch}
        state={mockState}
        dispatch={mockDispatch}
        undoSelection={mockUndoSelection}
      />
    );

    const cancelButton = screen.getByText(/Cancel/i);
    fireEvent.click(cancelButton);

    expect(mockHandleExit).toHaveBeenCalledTimes(1);
    component.unmount();
  });

  it("navigates back to mode selection", () => {
    const component = render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={() => {}}
        select={["/test.jpg"]}
        historyLocationSearch={mockHistoryLocationSearch}
        state={mockState}
        dispatch={mockDispatch}
        undoSelection={mockUndoSelection}
      />
    );

    const offsetRadio = screen.getByRole("radio", { name: /Correct incorrect camera timezone/i });
    fireEvent.click(offsetRadio);

    expect(screen.getByText(/Correct Camera Time/i)).toBeTruthy();

    const backButton = screen.getByText(/Back/i);
    fireEvent.click(backButton);

    expect(screen.getByText(/Shift Photo Timestamps/i)).toBeTruthy();
    component.unmount();
  });
});
