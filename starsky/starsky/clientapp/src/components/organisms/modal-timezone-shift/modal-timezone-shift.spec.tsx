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

  it("walks through timezone mode and completes execute-shift successfully", async () => {
    const mockCityTimezones = [
      { id: "America/New_York", displayName: "(UTC-05:00) New York" },
      { id: "Europe/Amsterdam", displayName: "(UTC+01:00) Amsterdam" }
    ];

    const mockTimezonePreview = [
      {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T20:32:00",
        delta: "+06:00:00",
        warning: "",
        error: ""
      }
    ];

    const mockExecuteResponse = {
      statusCode: 200,
      data: [
        {
          filePath: "/test.jpg",
          status: IExifStatus.Ok
        }
      ]
    };

    // Mock fetch for city timezones search
    jest.spyOn(FetchGet, "default").mockResolvedValue({
      statusCode: 200,
      data: mockCityTimezones
    });

    // Mock fetch for timezone preview
    const postSpy = jest.spyOn(FetchPost, "default").mockResolvedValue({
      statusCode: 200,
      data: mockTimezonePreview
    });

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

    // Step 1: Select timezone mode
    const timezoneRadio = await screen.findByRole("radio", {
      name: /I moved to a different place/i
    });
    await act(async () => {
      fireEvent.click(timezoneRadio);
    });

    // Wait for the "Change Location" text to appear
    await screen.findByText(/Change Location/i);

    // Step 2: Select original city (recorded timezone)
    const originalCityInputs = await screen.findAllByPlaceholderText(/Search or select/i);
    const originalCityInput = originalCityInputs[0];

    await act(async () => {
      fireEvent.change(originalCityInput, { target: { value: "New" } });
    });

    // Wait for dropdown to appear and select New York
    await screen.findByTestId("searchable-dropdown-item-America/New_York");
    const newYorkButton = screen
      .getByTestId("searchable-dropdown-item-America/New_York")
      .querySelector("button");

    await act(async () => {
      if (newYorkButton) fireEvent.click(newYorkButton);
    });

    // Step 3: Select new city (correct timezone)
    const newCityInputs = await screen.findAllByPlaceholderText(/Search or select/i);
    const newCityInput = newCityInputs[1];

    await act(async () => {
      fireEvent.change(newCityInput, { target: { value: "Amsterdam" } });
    });

    // Wait for dropdown and select Amsterdam
    await screen.findByTestId("searchable-dropdown-item-Europe/Amsterdam");
    const amsterdamButton = screen
      .getByTestId("searchable-dropdown-item-Europe/Amsterdam")
      .querySelector("button");

    await act(async () => {
      if (amsterdamButton) fireEvent.click(amsterdamButton);
    });

    // Step 4: Wait for preview to load
    await screen.findByText(/Preview/i);
    expect(screen.getByText(/Original:/i)).toBeTruthy();
    expect(screen.getByText(/New time:/i)).toBeTruthy();
    expect(screen.getByText(/Time shift:/i)).toBeTruthy();

    // Step 5: Mock execute-shift response
    postSpy.mockResolvedValueOnce({
      statusCode: 200,
      data: mockExecuteResponse.data
    });

    // Step 6: Click "Apply Shift" button
    const applyButton = await screen.findByText(/Apply Shift/i);
    expect(applyButton).not.toBeDisabled();

    await act(async () => {
      fireEvent.click(applyButton);
    });

    // Step 7: Verify execution was called and modal exits
    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 100));
    });

    expect(postSpy).toHaveBeenCalled();
    expect(mockDispatch).toHaveBeenCalled();
    expect(mockHandleExit).toHaveBeenCalled();

    component.unmount();
  });

  it("walks through offset mode and completes execute-shift successfully", async () => {
    const mockOffsetPreview = [
      {
        success: true,
        originalDateTime: "2024-08-12T14:32:00",
        correctedDateTime: "2024-08-12T17:32:00",
        delta: "+03:00:00",
        warning: "",
        error: ""
      }
    ];

    const mockExecuteResponse = {
      statusCode: 200,
      data: [
        {
          filePath: "/test.jpg",
          status: IExifStatus.Ok
        }
      ]
    };

    // Mock fetch for offset preview
    const postSpy = jest.spyOn(FetchPost, "default").mockResolvedValue({
      statusCode: 200,
      data: mockOffsetPreview
    });

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

    // Step 1: Select offset mode
    const offsetRadio = screen.getByRole("radio", { name: /Correct incorrect camera timezone/i });
    await act(async () => {
      fireEvent.click(offsetRadio);
    });

    expect(screen.getByText(/Correct Camera Time/i)).toBeTruthy();
    expect(screen.getByText(/Time offsets/i)).toBeTruthy();

    // Step 2: Set offset hours
    const hoursInput = screen.getByLabelText(/Hours/i);
    await act(async () => {
      fireEvent.change(hoursInput, { target: { value: "3" } });
    });

    // Step 3: Wait for preview to load
    await screen.findByText(/Preview/i);
    expect(screen.getByText(/Original:/i)).toBeTruthy();
    expect(screen.getByText(/Result:/i)).toBeTruthy();
    expect(screen.getByText(/Applied shift:/i)).toBeTruthy();

    // Verify the preview API was called
    expect(postSpy).toHaveBeenCalled();

    // Step 4: Mock execute-shift response
    postSpy.mockResolvedValueOnce({
      statusCode: 200,
      data: mockExecuteResponse.data
    });

    // Step 5: Click "Apply Shift" button
    const applyButton = screen.getByText(/Apply Shift/i);
    expect(applyButton).not.toBeDisabled();

    await act(async () => {
      fireEvent.click(applyButton);
    });

    // Step 6: Verify execution was called and modal exits
    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 100));
    });

    expect(postSpy).toHaveBeenCalled();
    expect(mockDispatch).toHaveBeenCalled();
    expect(mockHandleExit).toHaveBeenCalled();

    component.unmount();
  });
});
