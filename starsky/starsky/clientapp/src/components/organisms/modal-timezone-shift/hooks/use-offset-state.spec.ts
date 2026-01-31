import { renderHook } from "@testing-library/react";
import { act } from "react";
import { useOffsetState } from "./use-offset-state";

describe("useOffsetState", () => {
  it("reset sets all offsets to 0", () => {
    const { result } = renderHook(() => useOffsetState());

    // Set all offsets to non-zero values
    act(() => {
      result.current.setOffsetYears(2);
      result.current.setOffsetMonths(3);
      result.current.setOffsetDays(4);
      result.current.setOffsetHours(5);
      result.current.setOffsetMinutes(6);
      result.current.setOffsetSeconds(7);
    });

    // Call reset
    act(() => {
      result.current.reset();
    });

    expect(result.current.offsetYears).toBe(0);
    expect(result.current.offsetMonths).toBe(0);
    expect(result.current.offsetDays).toBe(0);
    expect(result.current.offsetHours).toBe(0);
    expect(result.current.offsetMinutes).toBe(0);
    expect(result.current.offsetSeconds).toBe(0);
  });
});
