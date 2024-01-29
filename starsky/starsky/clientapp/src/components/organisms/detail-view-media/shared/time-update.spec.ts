import { act } from "@testing-library/react";
import * as DateModule from "../../../../shared/date";
import { TimeUpdate } from "./time-update";

describe("TimeUpdate function", () => {
  let videoRef: React.RefObject<HTMLVideoElement>;
  let setIsLoadingMock: jest.Mock;
  let progressRef: React.RefObject<HTMLProgressElement>;
  let scrubberRef: React.RefObject<HTMLSpanElement>;
  let timeRef: React.RefObject<HTMLSpanElement>;
  let secondsToHoursSpy: jest.SpyInstance;

  beforeEach(() => {
    // Create fake elements
    const videoElement = document.createElement("video");
    const progressElement = document.createElement("progress");
    const scrubberElement = document.createElement("span");
    const timeElement = document.createElement("span");

    videoRef = { current: videoElement } as React.RefObject<HTMLVideoElement>;
    setIsLoadingMock = jest.fn();
    progressRef = {
      current: progressElement
    } as React.RefObject<HTMLProgressElement>;
    scrubberRef = {
      current: scrubberElement
    } as React.RefObject<HTMLSpanElement>;
    timeRef = { current: timeElement } as React.RefObject<HTMLSpanElement>;

    // Mock the SecondsToHours function from the shared/date module
    secondsToHoursSpy = jest.spyOn(DateModule, "SecondsToHours");
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it("should not update values if required refs are falsy", () => {
    // Set required refs to falsy values
    videoRef = { current: null };
    progressRef = { current: null };
    scrubberRef = { current: null };
    timeRef = { current: null };

    act(() => {
      TimeUpdate(videoRef, setIsLoadingMock, progressRef, scrubberRef, timeRef);
    });

    // Assert that no functions or properties were called/modified
    expect(progressRef.current).toBeNull();
    expect(scrubberRef.current).toBeNull();
    expect(timeRef.current).toBeNull();
    expect(setIsLoadingMock).not.toHaveBeenCalled();
    expect(secondsToHoursSpy).not.toHaveBeenCalled();
  });

  it("should update values and setIsLoading when refs are valid", () => {
    // Set up valid values for refs
    videoRef = { current: { currentTime: 10, duration: 100 } as any };

    act(() => {
      TimeUpdate(videoRef, setIsLoadingMock, progressRef, scrubberRef, timeRef);
    });

    // Assert that values were updated and setIsLoading was called
    expect(progressRef.current?.value).toBe(videoRef.current?.currentTime);
    expect(scrubberRef.current?.style.left).toBe("10%"); // Assuming scrubber left style is set in percentage
    expect(timeRef.current?.innerHTML).toBe("0:10 / 1:40"); // Assuming SecondsToHours returns formatted time
    expect(setIsLoadingMock).toHaveBeenCalledWith(false);
    expect(secondsToHoursSpy).toHaveBeenCalledTimes(2); // Once for current time, once for duration
  });

  it("should set max attribute for progress element if not set", () => {
    // Set up valid values for refs
    videoRef = {
      current: {
        duration: 100
      } as any
    };

    const setAttributeSpy = jest.fn();
    progressRef = {
      current: {
        value: 0,
        getAttribute: jest.fn(),
        setAttribute: setAttributeSpy
      } as any
    };

    act(() => {
      TimeUpdate(videoRef, setIsLoadingMock, progressRef, scrubberRef, timeRef);
    });

    // Assert that max attribute was set
    expect(setAttributeSpy).toHaveBeenCalledTimes(1);
    expect(setAttributeSpy).toHaveBeenCalledWith("max", "100");
  });
});
