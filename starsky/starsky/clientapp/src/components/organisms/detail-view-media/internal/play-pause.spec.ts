import { act } from "@testing-library/react";
import React from "react";
import { PlayPause } from "./play-pause";

describe("YourComponent", () => {
  let videoRef: React.RefObject<HTMLVideoElement>;
  let setIsErrorMock: jest.Mock;
  let setStartedMock: jest.Mock;
  let setPausedMock: jest.Mock;
  let setIsLoadingMock: jest.Mock;
  let videoElement: HTMLVideoElement;

  beforeEach(() => {
    videoElement = document.createElement("video");
    videoRef = {
      current: videoElement
    } as React.RefObject<HTMLVideoElement>;
    setIsErrorMock = jest.fn();
    setStartedMock = jest.fn();
    setPausedMock = jest.fn();
    setIsLoadingMock = jest.fn();

    // supress Error: Not implemented: HTMLMediaElement.prototype.pause
    jest.spyOn(window.HTMLMediaElement.prototype, "pause").mockImplementationOnce(() => {});

    jest
      .spyOn(window.HTMLMediaElement.prototype, "play")
      .mockImplementationOnce(() => Promise.resolve());
  });

  it("should handle the case when videoRef.current is falsy", () => {
    videoRef = { current: null };

    act(() => {
      PlayPause(
        videoRef,
        setIsErrorMock,
        "Error Message",
        setStartedMock,
        false,
        setPausedMock,
        setIsLoadingMock
      );
    });

    // Assert that no functions were called since videoRef.current is falsy
    expect(setIsErrorMock).not.toHaveBeenCalled();
    expect(setStartedMock).not.toHaveBeenCalled();
    expect(setPausedMock).not.toHaveBeenCalled();
    expect(setIsLoadingMock).not.toHaveBeenCalled();
  });

  it("should handle the case when videoRef.current.play is undefined", () => {
    const videoRef2 = {
      current: { ...videoElement, play: undefined }
    };

    act(() => {
      PlayPause(
        videoRef2 as any,
        setIsErrorMock,
        "Error Message",
        setStartedMock,
        false,
        setPausedMock,
        setIsLoadingMock
      );
    });

    // Assert that setIsError was called with the correct arguments
    expect(setIsErrorMock).toHaveBeenCalledWith("Error Message");

    // Assert that setStarted was called
    expect(setStartedMock).toHaveBeenCalledTimes(0);

    // Assert that no other functions were called
    expect(setPausedMock).not.toHaveBeenCalled();
    expect(setIsLoadingMock).not.toHaveBeenCalled();
  });

  it("should handle the case when paused is false", () => {
    act(() => {
      PlayPause(
        videoRef,
        setIsErrorMock,
        "Error Message",
        setStartedMock,
        false,
        setPausedMock,
        setIsLoadingMock
      );
    });

    // Assert that setStarted and play were called
    expect(setStartedMock).toHaveBeenCalled();

    // Assert that setPaused, setIsLoading, and setIsError were not called
    expect(setPausedMock).toHaveBeenCalled();
    expect(setIsLoadingMock).not.toHaveBeenCalled();
    expect(setIsErrorMock).not.toHaveBeenCalled();
  });

  it("should handle the case when paused is true", () => {
    act(() => {
      PlayPause(
        videoRef,
        setIsErrorMock,
        "Error Message",
        setStartedMock,
        false,
        setPausedMock,
        setIsLoadingMock
      );
    });

    // Assert that setPaused, videoRef.current.pause, setIsLoading, and setIsError were called
    expect(setPausedMock).toHaveBeenCalled();
    expect(setIsLoadingMock).toHaveBeenCalledTimes(0);
    expect(setIsErrorMock).not.toHaveBeenCalled(); // Since play resolves, this should not be called
  });
});
