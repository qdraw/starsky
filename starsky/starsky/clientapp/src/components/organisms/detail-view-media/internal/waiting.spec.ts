import { RefObject } from "react";
import { act } from "react"; // Import 'act' to handle async updates
import { Waiting } from "./waiting";

describe("Waiting function", () => {
  it("should set isLoading to true when video is loading", () => {
    // Create a mock video element
    const videoRefMock = {
      current: {
        networkState: 2,
        NETWORK_LOADING: 2
      }
    } as RefObject<HTMLVideoElement>;

    // Create a mock set state function
    const setIsLoadingMock = jest.fn();

    // Render the component and call the Waiting function
    act(() => {
      Waiting(videoRefMock, setIsLoadingMock);
    });

    // Assert that setIsLoadingMock was called with true
    expect(setIsLoadingMock).toHaveBeenCalledWith(true);
  });

  it("should not set isLoading to true when video is not loading", () => {
    // Create a mock video element
    const videoRefMock = {
      current: {
        networkState: 1,
        NETWORK_LOADING: 2
      }
    } as RefObject<HTMLVideoElement>;

    // Create a mock set state function
    const setIsLoadingMock = jest.fn();

    // Render the component and call the Waiting function
    act(() => {
      Waiting(videoRefMock, setIsLoadingMock);
    });

    // Assert that setIsLoadingMock was not called
    expect(setIsLoadingMock).not.toHaveBeenCalled();
  });

  it("should not set isLoading to true if videoRef.current is falsy", () => {
    // Create a falsy videoRef
    const videoRefMock = {
      current: null
    };

    // Create a mock set state function
    const setIsLoadingMock = jest.fn();

    // Render the component and call the Waiting function
    act(() => {
      Waiting(videoRefMock, setIsLoadingMock);
    });

    // Assert that setIsLoadingMock was not called
    expect(setIsLoadingMock).not.toHaveBeenCalled();
  });
});
