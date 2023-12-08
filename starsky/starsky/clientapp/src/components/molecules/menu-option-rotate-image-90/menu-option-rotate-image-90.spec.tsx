import {
  render,
  fireEvent,
  waitFor,
  act,
  screen
} from "@testing-library/react";
import MenuOptionRotateImage90 from "./menu-option-rotate-image-90.tsx";
import { IDetailView } from "../../../interfaces/IDetailView.ts"; // for expect assertions

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
});
