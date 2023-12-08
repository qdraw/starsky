import {
  act,
  fireEvent,
  render,
  screen,
  waitFor
} from "@testing-library/react";
import { IDetailView } from "../../../interfaces/IDetailView.ts";
import {
  IFileIndexItem,
  Orientation
} from "../../../interfaces/IFileIndexItem.ts";
import * as FetchGet from "../../../shared/fetch-get.ts"; // for expect assertions
import MenuOptionRotateImage90, {
  requestNewFileHash
} from "./menu-option-rotate-image-90.tsx";

describe("MenuOptionRotateImage90", () => {
  const mockState = {
    subPath: "/path/to/image.jpg",
    fileIndexItem: {
      fileHash: "abc123",
      filePath: "/path/to/image.jpg",
      orientation: "Horizontal"
      // Add any other necessary properties for your test case
    },
    breadcrumb: {},
    pageType: "detailView",
    colorClassUsage: [],
    relativeObjects: {},
    colorClassActiveList: [],
    colorClassSelect: [],
    isReadOnly: false,
    dateCache: {}
    // Add any other necessary properties for your test case
  } as unknown as IDetailView;

  it("renders without crashing", () => {
    const container = render(
      <MenuOptionRotateImage90
        state={mockState}
        setIsLoading={jest.fn()}
        dispatch={jest.fn()}
        isMarkedAsDeleted={false}
        isReadOnly={false}
      />
    );
    expect(container.container).toBeTruthy();

    container.unmount();
  });

  it("calls rotateImage90 function on click", async () => {
    const setIsLoadingMock = jest.fn();
    const container = render(
      <MenuOptionRotateImage90
        state={mockState}
        setIsLoading={setIsLoadingMock}
        dispatch={jest.fn()}
        isMarkedAsDeleted={false}
        isReadOnly={false}
      />
    );

    fireEvent.click(container.getByTestId("rotate")); // Assuming you have "data-testid" attribute in your MenuOption component

    // You might need to wait for async operations to complete before asserting
    await waitFor(() => {
      expect(setIsLoadingMock).toHaveBeenCalledWith(true);
      // Add more assertions based on the expected behavior of your component
    });
  });

  it("handles timeouts correctly", async () => {
    const setIsLoadingMock = jest.fn();

    const component = render(
      <MenuOptionRotateImage90
        state={mockState}
        setIsLoading={setIsLoadingMock}
        dispatch={jest.fn()}
        isMarkedAsDeleted={false}
        isReadOnly={false}
      />
    );

    fireEvent.click(screen.getByTestId("rotate"));

    // Manually advance the timer to the first setTimeout
    act(() => {
      jest.advanceTimersByTime(3000);
    });

    expect(setIsLoadingMock).toHaveBeenCalled();

    // Manually advance the timer to the second setTimeout
    act(() => {
      jest.advanceTimersByTime(7000);
    });

    expect(setIsLoadingMock).toHaveBeenCalledWith(true);

    component.unmount();
  });

  describe("requestNewFileHash", () => {
    const mockState: IDetailView = {
      subPath: "/path/to/image.jpg",
      fileIndexItem: {
        fileHash: "abc123",
        filePath: "/path/to/image.jpg",
        orientation: Orientation.Horizontal
      } as unknown as IFileIndexItem
    } as unknown as IDetailView;

    const mockSetIsLoading = jest.fn();
    const mockDispatch = jest.fn();

    let fetchGetSpy: jest.SpyInstance;

    beforeEach(() => {
      fetchGetSpy = jest.spyOn(FetchGet, "default");
    });

    afterEach(() => {
      jest.clearAllMocks();
    });

    it("returns null if FetchGet fails", async () => {
      fetchGetSpy.mockResolvedValueOnce(null);

      const result = await requestNewFileHash(
        mockState,
        mockSetIsLoading,
        mockDispatch
      );

      expect(result).toBeNull();
      expect(mockSetIsLoading).toHaveBeenCalledTimes(0);
    });

    it("returns null if FetchGet status code is not 200", async () => {
      const mockResult = {
        statusCode: 404 // or any non-200 status code
      };
      fetchGetSpy.mockResolvedValueOnce(mockResult);

      const result = await requestNewFileHash(
        mockState,
        mockSetIsLoading,
        mockDispatch
      );

      expect(result).toBeNull();
      expect(mockSetIsLoading).toHaveBeenCalledWith(false);
    });

    it("updates context and returns true when fileHash changes", async () => {
      const mockResult = {
        statusCode: 200,
        data: {
          fileIndexItem: {
            fileHash: "test"
          },
          pageType: "DetailView"
        }
      };
      fetchGetSpy.mockResolvedValueOnce(mockResult);

      const result = await requestNewFileHash(
        mockState,
        mockSetIsLoading,
        mockDispatch
      );

      expect(result).toBe(true);
      expect(mockDispatch).toHaveBeenCalledWith({
        fileHash: "test",
        filePath: undefined,
        orientation: "Horizontal",
        type: "update"
      });
      expect(mockSetIsLoading).toHaveBeenCalledWith(false);
    });

    it("returns false and updates context when fileHash remains the same", async () => {
      const mockResult = {
        statusCode: 200,
        data: {
          fileIndexItem: {
            fileHash: "abc123" // same as mockState.fileIndexItem.fileHash
          },
          pageType: "DetailView"
        }
      };
      fetchGetSpy.mockResolvedValueOnce(mockResult);

      const result = await requestNewFileHash(
        mockState,
        mockSetIsLoading,
        mockDispatch
      );

      expect(result).toBe(false);
      expect(mockDispatch).toHaveBeenCalledTimes(0);
      expect(mockSetIsLoading).toHaveBeenCalledTimes(0);
    });
  });
});
