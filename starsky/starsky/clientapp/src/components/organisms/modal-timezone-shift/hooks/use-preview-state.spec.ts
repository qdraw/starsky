import { renderHook } from "@testing-library/react";
import { act } from "react";
import { newIFileIndexItem } from "../../../../interfaces/IFileIndexItem";
import { usePreviewState } from "./use-preview-state";

describe("usePreviewState", () => {
  it("previewReset resets all preview state", () => {
    const { result } = renderHook(() => usePreviewState());

    // Set all state to non-default values
    act(() => {
      result.current.setPreview({
        offsetData: [
          {
            originalDateTime: "2023-01-01T12:00:00Z",
            correctedDateTime: "2023-01-02T12:00:00Z",
            warning: "Sample warning",
            error: "Sample error",
            success: false,
            delta: "1 day",
            fileIndexItem: newIFileIndexItem()
          }
        ],
        timezoneData: [
          {
            originalDateTime: "2023-01-01T12:00:00Z",
            correctedDateTime: "2023-01-02T12:00:00Z",
            warning: "Sample warning",
            error: "Sample error",
            success: false,
            delta: "1 day",
            fileIndexItem: newIFileIndexItem()
          }
        ]
      });
      result.current.setIsLoadingPreview(true);
      result.current.setIsExecuting(true);
      result.current.setError("error");
    });

    // Call previewReset
    act(() => {
      result.current.previewReset();
    });

    expect(result.current.preview).toEqual({ offsetData: [], timezoneData: [] });
    expect(result.current.isLoadingPreview).toBe(false);
    expect(result.current.isExecuting).toBe(false);
    expect(result.current.error).toBeNull();
  });
});
