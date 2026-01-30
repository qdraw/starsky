import { fireEvent, render, screen, waitFor } from "@testing-library/react";
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
          fileName: "test.jpg",
          parentDirectory: "/",
          status: IExifStatus.Ok
        }
      ]
    };

    // Mock fetch for city timezones search
    const getSpy = jest.spyOn(FetchGet, "default").mockReset().mockResolvedValue({
      statusCode: 200,
      data: mockCityTimezones
    });

    // Mock fetch for timezone preview
    const postSpy = jest.spyOn(FetchPost, "default").mockReset().mockResolvedValue({
      statusCode: 200,
      data: mockTimezonePreview
    });

    const mockHandleExit = jest.fn();

    const component = render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={mockHandleExit}
        select={["test.jpg"]}
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

    // Step 4: Both timezones are now selected
    // Verify both input fields have values
    const inputs = screen.getAllByPlaceholderText(/Search or select/i);
    expect(inputs[0]).toHaveValue("(UTC-05:00) New York");
    expect(inputs[1]).toHaveValue("(UTC+01:00) Amsterdam");

    // Give a moment for preview generation to complete if it happens
    // it'll wait until the mock function has been called once.
    await waitFor(() => expect(getSpy).toHaveBeenCalledTimes(5));

    // Step 5: Mock execute-shift response
    postSpy.mockResolvedValueOnce({
      statusCode: 200,
      data: mockExecuteResponse.data
    });

    // Step 6: Click "Apply Shift" button
    const applyButton = screen.getByText(/Apply Shift/i);

    await act(async () => {
      fireEvent.click(applyButton);
    });

    // Step 7: Verify execution was called and modal exits
    await act(async () => {
      await waitFor(() => expect(mockDispatch).toHaveBeenCalled());
    });

    expect(mockDispatch).toHaveBeenCalled();

    expect(postSpy).toHaveBeenCalledTimes(3);
    expect(postSpy).toHaveBeenNthCalledWith(
      1,
      "/starsky/api/meta-time-correct/timezone-preview?f=/test.jpg&collections=true",
      '{"recordedTimezoneId":"America/New_York","correctTimezoneId":""}',
      "post",
      { "Content-Type": "application/json" }
    );

    expect(postSpy).toHaveBeenNthCalledWith(
      2,
      "/starsky/api/meta-time-correct/timezone-preview?f=/test.jpg&collections=true",
      '{"recordedTimezoneId":"Europe/Amsterdam","correctTimezoneId":"America/New_York"}',
      "post",
      { "Content-Type": "application/json" }
    );

    expect(postSpy).toHaveBeenNthCalledWith(
      3,
      "/starsky/api/meta-time-correct/timezone-execute?f=/test.jpg&collections=true",
      '{"recordedTimezoneId":"America/New_York","correctTimezoneId":"Europe/Amsterdam"}',
      "post",
      { "Content-Type": "application/json" }
    );

    component.unmount();
  }, 10000);

  describe.each([
    {
      label: "Years",
      payload: { year: 2, month: 0, day: 0, hour: 0, minute: 0, second: 0 }
    },
    {
      label: "Months",
      payload: { year: 0, month: 2, day: 0, hour: 0, minute: 0, second: 0 }
    },
    {
      label: "Days",
      payload: { year: 0, month: 0, day: 2, hour: 0, minute: 0, second: 0 }
    },
    {
      label: "Hours",
      payload: { year: 0, month: 0, day: 0, hour: 2, minute: 0, second: 0 }
    },
    {
      label: "Minutes",
      payload: { year: 0, month: 0, day: 0, hour: 0, minute: 2, second: 0 }
    },
    {
      label: "Seconds",
      payload: { year: 0, month: 0, day: 0, hour: 0, minute: 0, second: 2 }
    }
  ])("offset mode: $label", ({ label, payload }) => {
    it(`completes execute-shift successfully for ${label}`, async () => {
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
            fileName: "test.jpg",
            parentDirectory: "/",
            status: IExifStatus.Ok
          }
        ]
      };

      // Mock fetch for offset preview
      const postSpy = jest.spyOn(FetchPost, "default").mockReset().mockResolvedValue({
        statusCode: 200,
        data: mockOffsetPreview
      });

      const mockHandleExit = jest.fn();

      const component = render(
        <ModalTimezoneShift
          isOpen={true}
          handleExit={mockHandleExit}
          select={["test.jpg"]}
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

      // Step 2: Set offset field(s)
      for (const [key, value] of Object.entries(payload)) {
        if (value !== 0) {
          const input = screen.getByLabelText(new RegExp(key, "i"));
          await act(async () => {
            fireEvent.change(input, { target: { value: String(value) } });
          });
        }
      }

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

      expect(postSpy).toHaveBeenCalledTimes(2);
      const expectedPreviewPayload = JSON.stringify(payload);
      expect(postSpy).toHaveBeenNthCalledWith(
        1,
        "/starsky/api/meta-time-correct/offset-preview?f=/test.jpg&collections=false",
        expectedPreviewPayload,
        "post",
        { "Content-Type": "application/json" }
      );

      expect(postSpy).toHaveBeenNthCalledWith(
        2,
        "/starsky/api/meta-time-correct/offset-execute?f=/test.jpg&collections=true",
        expectedPreviewPayload,
        "post",
        { "Content-Type": "application/json" }
      );

      expect(mockDispatch).toHaveBeenCalled();
      expect(mockHandleExit).toHaveBeenCalled();

      component.unmount();
    });
  });

  it("offset mode: combination of all fields", async () => {
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
          fileName: "test.jpg",
          parentDirectory: "/",
          status: IExifStatus.Ok
        }
      ]
    };

    // Mock fetch for offset preview
    const postSpy = jest.spyOn(FetchPost, "default").mockReset().mockResolvedValue({
      statusCode: 200,
      data: mockOffsetPreview
    });

    const mockHandleExit = jest.fn();

    const component = render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={mockHandleExit}
        select={["test.jpg"]}
        historyLocationSearch={mockHistoryLocationSearch}
        state={mockState}
        dispatch={mockDispatch}
        undoSelection={mockUndoSelection}
      />
    );

    // Step 1: Select offset mode
    const offsetRadio = screen.getByRole("radio", {
      name: /Correct incorrect camera timezone/i
    });
    await act(async () => {
      fireEvent.click(offsetRadio);
    });

    expect(screen.getByText(/Correct Camera Time/i)).toBeTruthy();
    expect(screen.getByText(/Time offsets/i)).toBeTruthy();

    // Step 2: Set all offset fields
    const values = { year: 1, month: 2, day: 3, hour: 4, minute: 5, second: 6 };
    for (const [key, value] of Object.entries(values)) {
      const input = screen.getByLabelText(new RegExp(key, "i"));
      await act(async () => {
        fireEvent.change(input, { target: { value: String(value) } });
      });
    }

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

    expect(postSpy).toHaveBeenCalledTimes(7);
    expect(postSpy).toHaveBeenNthCalledWith(
      1,
      "/starsky/api/meta-time-correct/offset-preview?f=/test.jpg&collections=false",
      '{"year":1,"month":0,"day":0,"hour":0,"minute":0,"second":0}',
      "post",
      { "Content-Type": "application/json" }
    );

    expect(postSpy).toHaveBeenNthCalledWith(
      2,
      "/starsky/api/meta-time-correct/offset-preview?f=/test.jpg&collections=false",
      '{"year":1,"month":2,"day":0,"hour":0,"minute":0,"second":0}',
      "post",
      { "Content-Type": "application/json" }
    );

    expect(postSpy).toHaveBeenNthCalledWith(
      3,
      "/starsky/api/meta-time-correct/offset-preview?f=/test.jpg&collections=false",
      '{"year":1,"month":2,"day":3,"hour":0,"minute":0,"second":0}',
      "post",
      { "Content-Type": "application/json" }
    );

    expect(postSpy).toHaveBeenNthCalledWith(
      4,
      "/starsky/api/meta-time-correct/offset-preview?f=/test.jpg&collections=false",
      '{"year":1,"month":2,"day":3,"hour":4,"minute":0,"second":0}',
      "post",
      { "Content-Type": "application/json" }
    );

    expect(postSpy).toHaveBeenNthCalledWith(
      5,
      "/starsky/api/meta-time-correct/offset-preview?f=/test.jpg&collections=false",
      '{"year":1,"month":2,"day":3,"hour":4,"minute":5,"second":0}',
      "post",
      { "Content-Type": "application/json" }
    );

    expect(postSpy).toHaveBeenNthCalledWith(
      6,
      "/starsky/api/meta-time-correct/offset-preview?f=/test.jpg&collections=false",
      '{"year":1,"month":2,"day":3,"hour":4,"minute":5,"second":6}',
      "post",
      { "Content-Type": "application/json" }
    );

    // Execute call!!
    expect(postSpy).toHaveBeenNthCalledWith(
      7,
      "/starsky/api/meta-time-correct/offset-execute?f=/test.jpg&collections=true",
      '{"year":1,"month":2,"day":3,"hour":4,"minute":5,"second":6}',
      "post",
      { "Content-Type": "application/json" }
    );

    expect(mockDispatch).toHaveBeenCalled();
    expect(mockHandleExit).toHaveBeenCalled();

    component.unmount();
  });

  it("offset mode: combination of all fields invalid input", async () => {
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

    // Mock fetch for offset preview
    const postSpy = jest.spyOn(FetchPost, "default").mockReset().mockResolvedValue({
      statusCode: 200,
      data: mockOffsetPreview
    });

    const mockHandleExit = jest.fn();

    const component = render(
      <ModalTimezoneShift
        isOpen={true}
        handleExit={mockHandleExit}
        select={["test.jpg"]}
        historyLocationSearch={mockHistoryLocationSearch}
        state={mockState}
        dispatch={mockDispatch}
        undoSelection={mockUndoSelection}
      />
    );

    // Step 1: Select offset mode
    const offsetRadio = screen.getByRole("radio", {
      name: /Correct incorrect camera timezone/i
    });
    await act(async () => {
      fireEvent.click(offsetRadio);
    });

    expect(screen.getByText(/Correct Camera Time/i)).toBeTruthy();
    expect(screen.getByText(/Time offsets/i)).toBeTruthy();

    // Step 2: Set all offset fields
    const values = { year: "t0", month: "t0", day: "t0", hour: "t0", minute: "t0", second: "t0" };
    for (const [key, value] of Object.entries(values)) {
      const input = screen.getByLabelText(new RegExp(key, "i"));
      await act(async () => {
        fireEvent.change(input, { target: { value: String(value) } });
      });
    }

    expect(postSpy).toHaveBeenCalledTimes(6);

    for (let index = 1; index < 6; index++) {
      expect(postSpy).toHaveBeenNthCalledWith(
        index,
        "/starsky/api/meta-time-correct/offset-preview?f=/test.jpg&collections=false",
        '{"year":0,"month":0,"day":0,"hour":0,"minute":0,"second":0}',
        "post",
        { "Content-Type": "application/json" }
      );
    }

    component.unmount();
  });
});
