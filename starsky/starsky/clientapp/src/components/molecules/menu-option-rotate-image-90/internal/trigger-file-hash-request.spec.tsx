import { act } from "react";
import { IDetailView } from "../../../../interfaces/IDetailView";
import { Orientation } from "../../../../interfaces/IFileIndexItem";
import * as RequestNewFileHash from "./request-new-filehash";
import { TriggerFileHashRequest } from "./trigger-file-hash-request";

describe("TriggerFileHashRequest", () => {
  const state: IDetailView = {
    subPath: "/test/image.jpg",
    fileIndexItem: {
      fileHash: "123",
      filePath: "/test/image.jpg",
      orientation: Orientation.Horizontal
    }
  } as IDetailView;

  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  it("returns immediately when the file hash changes on the first attempt", () => {
    const requestSpy = jest
      .spyOn(RequestNewFileHash, "RequestNewFileHash")
      .mockResolvedValueOnce(true);

    const setIsLoading = jest.fn();
    TriggerFileHashRequest(state, setIsLoading, jest.fn(), 1, 1);

    act(() => {
      jest.advanceTimersByTime(100);
    });

    jest.advanceTimersByTime(1);
    jest.runAllTimers();
    expect(requestSpy).toHaveBeenCalledTimes(1);
  });

  it("retry when failed", () => {
    const requestSpy = jest
      .spyOn(RequestNewFileHash, "RequestNewFileHash")
      .mockResolvedValueOnce(false);

    const setIsLoading = jest.fn();
    TriggerFileHashRequest(state, setIsLoading, jest.fn(), 1, 1);

    act(() => {
      jest.advanceTimersByTime(100);
    });

    jest.advanceTimersByTime(1);
    jest.runAllTimers();
    expect(requestSpy).toHaveBeenCalledTimes(2);
  });

  it("retry when failed (max 3 times)", () => {
    const requestSpy = jest
      .spyOn(RequestNewFileHash, "RequestNewFileHash")
      .mockResolvedValueOnce(false)
      .mockResolvedValueOnce(false)
      .mockResolvedValueOnce(false)
      .mockResolvedValueOnce(false);

    const setIsLoading = jest.fn();
    TriggerFileHashRequest(state, setIsLoading, jest.fn(), 1, 1);

    act(() => {
      jest.advanceTimersByTime(100);
    });

    jest.advanceTimersByTime(1);
    jest.runAllTimers();
    expect(requestSpy).toHaveBeenCalledTimes(3);
  });
});
