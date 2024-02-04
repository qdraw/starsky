import { act, fireEvent, render } from "@testing-library/react";
import React from "react";
import { ProgressBar } from "./progress-bar";
import * as UpdateProgressByClick from "./update-progress-by-click";

describe("ProgressBar component", () => {
  let scrubberRef: React.RefObject<HTMLSpanElement>;
  let progressRef: React.RefObject<HTMLProgressElement>;
  let videoRef: React.RefObject<HTMLVideoElement>;

  beforeEach(() => {
    scrubberRef = {
      current: document.createElement("span")
    } as React.RefObject<HTMLSpanElement>;
    progressRef = {
      current: document.createElement("progress")
    } as React.RefObject<HTMLProgressElement>;
    videoRef = {
      current: document.createElement("video")
    } as React.RefObject<HTMLVideoElement>;
  });

  it("should call UpdateProgressByClick on click event", () => {
    // Spy on the UpdateProgressByClick function
    const updateProgressSpy = jest.spyOn(UpdateProgressByClick, "UpdateProgressByClick");

    const { getByTestId } = render(
      <ProgressBar scrubberRef={scrubberRef} progressRef={progressRef} videoRef={videoRef} />
    );

    // Click on the progress bar
    act(() => {
      fireEvent.click(getByTestId("progress-element"));
    });

    // Assert that UpdateProgressByClick was called with the correct arguments
    expect(updateProgressSpy).toHaveBeenCalledWith(videoRef, expect.anything());

    // Restore the original function after the test
    updateProgressSpy.mockRestore();
  });
});
