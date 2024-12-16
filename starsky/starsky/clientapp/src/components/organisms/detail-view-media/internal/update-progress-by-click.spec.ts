import * as GetMousePositionModule from "./get-mouse-position";
import { UpdateProgressByClick } from "./update-progress-by-click";

describe("UpdateProgressByClick function", () => {
  let videoRef: React.RefObject<HTMLVideoElement>;
  let getMousePositionSpy: jest.SpyInstance;

  beforeEach(() => {
    // Create a fake video element
    const videoElement = document.createElement("video");
    videoRef = { current: videoElement } as React.RefObject<HTMLVideoElement>;

    // Spy on the GetMousePosition function
    getMousePositionSpy = jest.spyOn(GetMousePositionModule, "GetMousePosition");
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it("should update currentTime when videoRef and event.target are valid", () => {
    // Mock event
    const event = {
      target: document.createElement("video") as unknown as EventTarget
    } as React.MouseEvent;

    // Mock GetMousePosition to return a valid value
    getMousePositionSpy.mockReturnValue(0.5);

    // Set up video duration
    videoRef = {
      current: {
        duration: 100
      } as HTMLVideoElement
    };

    // Call the function
    UpdateProgressByClick(videoRef, event);

    // Assert that currentTime was updated
    expect(videoRef.current?.currentTime).toBe(50);

    // Assert that GetMousePosition was called with the correct arguments
    expect(getMousePositionSpy).toHaveBeenCalledWith(event);
  });

  it("should not update currentTime when event.target is not valid", () => {
    // Mock invalid event
    const event = {} as React.MouseEvent;

    // Call the function
    UpdateProgressByClick(videoRef, event);

    // Assert that currentTime was not updated
    expect(videoRef.current?.currentTime).toBe(0);

    // Assert that GetMousePosition was not called
    expect(getMousePositionSpy).not.toHaveBeenCalled();
  });

  it("should not update currentTime when videoRef is not valid", () => {
    // Mock valid event
    const event = {
      target: document.createElement("div") as unknown as EventTarget
    } as React.MouseEvent;

    // Set videoRef to null
    videoRef = {
      current: null
    };

    // Call the function
    UpdateProgressByClick(videoRef, event);

    // Assert that currentTime was not updated
    expect(videoRef.current).toBeNull();

    // Assert that GetMousePosition was not called
    expect(getMousePositionSpy).not.toHaveBeenCalled();
  });
});
