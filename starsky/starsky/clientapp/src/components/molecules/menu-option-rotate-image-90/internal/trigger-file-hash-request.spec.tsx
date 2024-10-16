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

  it("returns immediately when the file hash changes on the first attempt", () => {
    jest.useFakeTimers();

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
    jest.useRealTimers();
  });
});
