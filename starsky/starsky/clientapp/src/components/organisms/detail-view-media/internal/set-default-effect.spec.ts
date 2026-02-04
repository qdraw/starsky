import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";
import { SetDefaultEffect } from "./set-default-effect";

describe("setDefaultEffect function", () => {
  let historyLocationSearch: string;
  let setDownloadPhotoApiMock: jest.Mock;
  let videoRef: React.RefObject<HTMLVideoElement>;
  let scrubberRef: React.RefObject<HTMLSpanElement>;
  let progressRef: React.RefObject<HTMLProgressElement>;
  let timeRef: React.RefObject<HTMLSpanElement>;

  beforeEach(() => {
    historyLocationSearch = "example-search";
    setDownloadPhotoApiMock = jest.fn();
    videoRef = {
      current: document.createElement("video")
    } as React.RefObject<HTMLVideoElement>;
    scrubberRef = {
      current: document.createElement("span")
    } as React.RefObject<HTMLSpanElement>;
    progressRef = {
      current: document.createElement("progress")
    } as React.RefObject<HTMLProgressElement>;
    timeRef = {
      current: document.createElement("span")
    } as React.RefObject<HTMLSpanElement>;
  });

  it("should not perform actions when any of the refs are falsy", () => {
    // Set one ref to null to simulate a falsy condition
    scrubberRef = {
      current: null
    };

    SetDefaultEffect(
      historyLocationSearch,
      setDownloadPhotoApiMock,
      videoRef,
      scrubberRef,
      progressRef,
      timeRef
    );

    // Assert that setDownloadPhotoApiMock was called with the correct argument
    const downloadApiLocal = new UrlQuery().UrlDownloadPhotoApi(
      new URLPath().encodeURI(new URLPath().getFilePath(historyLocationSearch)),
      false,
      true
    );

    expect(setDownloadPhotoApiMock).toHaveBeenCalledWith(downloadApiLocal);

    // Assert that no other actions were performed
    expect(videoRef.current?.getAttribute("src")).toBeNull();
    expect(videoRef.current?.currentTime).toBe(0);
    expect(scrubberRef.current).toBeNull();
    expect(progressRef.current?.getAttribute("max")).toBeNull();
    expect(progressRef.current?.value).toBe(0);
    expect(timeRef.current?.innerHTML).toBe("");
  });

  it("should perform actions when all refs are valid", () => {
    SetDefaultEffect(
      historyLocationSearch,
      setDownloadPhotoApiMock,
      videoRef,
      scrubberRef,
      progressRef,
      timeRef
    );

    // Assert that setDownloadPhotoApiMock was called with the correct argument

    const downloadApiLocal = new UrlQuery().UrlDownloadPhotoApi(
      new URLPath().encodeURI(new URLPath().getFilePath(historyLocationSearch)),
      false,
      true
    );

    expect(setDownloadPhotoApiMock).toHaveBeenCalledWith(downloadApiLocal);

    // Assert that all actions were performed
    expect(videoRef.current?.getAttribute("src")).toBe(downloadApiLocal);
    expect(videoRef.current?.currentTime).toBe(0);
    expect(scrubberRef.current?.style.left).toBe("0%");
    expect(progressRef.current?.getAttribute("max")).toBeNull();
    expect(progressRef.current?.value).toBe(0);
    expect(timeRef.current?.innerHTML).toBe("");
  });

  // Add more test cases as needed

  // For example, you might want to test additional scenarios:
  // - Multiple refs are falsy
  // - All refs are valid
  // - Other actions performed after the if statement
});
