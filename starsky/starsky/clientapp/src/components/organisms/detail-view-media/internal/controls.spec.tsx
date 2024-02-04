import { act, fireEvent, render } from "@testing-library/react";
import React from "react";
import { Controls } from "./controls";
import * as PlayPauseModule from "./play-pause"; // Import the entire module to spy on its methods

describe("Controls component", () => {
  let scrubberRef: React.RefObject<HTMLSpanElement>;
  let progressRef: React.RefObject<HTMLProgressElement>;
  let videoRef: React.RefObject<HTMLVideoElement>;
  let timeRef: React.RefObject<HTMLSpanElement>;

  beforeEach(() => {
    // supress Error: Not implemented: HTMLMediaElement.prototype.pause
    jest.spyOn(window.HTMLMediaElement.prototype, "pause").mockImplementationOnce(() => {});

    jest
      .spyOn(window.HTMLMediaElement.prototype, "play")
      .mockImplementationOnce(() => Promise.resolve());

    scrubberRef = {
      current: document.createElement("span")
    } as React.RefObject<HTMLSpanElement>;
    progressRef = {
      current: document.createElement("progress")
    } as React.RefObject<HTMLProgressElement>;
    videoRef = {
      current: document.createElement("video")
    } as React.RefObject<HTMLVideoElement>;
    timeRef = {
      current: document.createElement("span")
    } as React.RefObject<HTMLSpanElement>;
  });

  it("should call PlayPause on button click", () => {
    // Spy on the PlayPause method from the play-pause module
    const playPauseSpy = jest.spyOn(PlayPauseModule, "PlayPause");

    const { getByText } = render(
      <Controls
        scrubberRef={scrubberRef}
        progressRef={progressRef}
        videoRef={videoRef}
        paused={false}
        setPaused={jest.fn()}
        setIsError={jest.fn()}
        setStarted={jest.fn()}
        setIsLoading={jest.fn()}
        timeRef={timeRef}
      />
    );

    // Click on the Play/Pause button
    act(() => {
      fireEvent.click(getByText(/Pause/)); // Assuming the button text is "Pause" when not paused
    });

    // Assert that PlayPause was called with the correct arguments
    expect(playPauseSpy).toHaveBeenCalledWith(
      videoRef,
      expect.any(Function),
      expect.any(String),
      expect.any(Function),
      false,
      expect.any(Function),
      expect.any(Function)
    );

    // Restore the original method after the test
    playPauseSpy.mockRestore();
  });

  it("should call PlayPause on Enter key press", () => {
    // Spy on the PlayPause method from the play-pause module
    const playPauseSpy = jest.spyOn(PlayPauseModule, "PlayPause");

    const { getByText } = render(
      <Controls
        scrubberRef={scrubberRef}
        progressRef={progressRef}
        videoRef={videoRef}
        paused={true}
        setPaused={jest.fn()}
        setIsError={jest.fn()}
        setStarted={jest.fn()}
        setIsLoading={jest.fn()}
        timeRef={timeRef}
      />
    );

    // Press Enter key on the Play/Pause button
    act(() => {
      fireEvent.keyDown(getByText(/Play/), { key: "Enter" }); // Assuming the button text is "Play" when paused
    });

    // Assert that PlayPause was called with the correct arguments
    expect(playPauseSpy).toHaveBeenCalledWith(
      videoRef,
      expect.any(Function),
      expect.any(String),
      expect.any(Function),
      true,
      expect.any(Function),
      expect.any(Function)
    );

    // Restore the original method after the test
    playPauseSpy.mockRestore();
  });
});
