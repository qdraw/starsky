import { renderHook } from "@testing-library/react";
import { act } from "react";
import { useTimezoneState } from "./use-timezone-state";

describe("useTimezoneState", () => {
  it("reset sets all timezone state to empty string", () => {
    const { result } = renderHook(() => useTimezoneState());

    // Set all state to non-default values
    act(() => {
      result.current.setRecordedTimezoneId("rec");
      result.current.setCorrectTimezoneId("cor");
      result.current.setRecordedTimezoneDisplayName("recName");
      result.current.setCorrectTimezoneDisplayName("corName");
    });

    // Call reset
    act(() => {
      result.current.reset();
    });

    expect(result.current.recordedTimezoneId).toBe("");
    expect(result.current.correctTimezoneId).toBe("");
    expect(result.current.recordedTimezoneDisplayName).toBe("");
    expect(result.current.correctTimezoneDisplayName).toBe("");
  });
});
