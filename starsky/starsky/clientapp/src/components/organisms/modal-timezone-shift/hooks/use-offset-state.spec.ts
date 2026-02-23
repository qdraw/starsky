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

  describe("getOffset", () => {
    describe.each([
      {
        scenario: "all zeros initially",
        setup: [],
        expected: { years: 0, months: 0, days: 0, hours: 0, minutes: 0, seconds: 0 }
      },
      {
        scenario: "all values set",
        setup: [
          { setter: "setOffsetYears", value: 2 },
          { setter: "setOffsetMonths", value: 3 },
          { setter: "setOffsetDays", value: 4 },
          { setter: "setOffsetHours", value: 5 },
          { setter: "setOffsetMinutes", value: 6 },
          { setter: "setOffsetSeconds", value: 7 }
        ],
        expected: { years: 2, months: 3, days: 4, hours: 5, minutes: 6, seconds: 7 }
      },
      {
        scenario: "negative values",
        setup: [
          { setter: "setOffsetYears", value: -1 },
          { setter: "setOffsetMonths", value: -2 },
          { setter: "setOffsetDays", value: -3 },
          { setter: "setOffsetHours", value: -4 },
          { setter: "setOffsetMinutes", value: -5 },
          { setter: "setOffsetSeconds", value: -6 }
        ],
        expected: { years: -1, months: -2, days: -3, hours: -4, minutes: -5, seconds: -6 }
      },
      {
        scenario: "mixed positive and negative values",
        setup: [
          { setter: "setOffsetYears", value: 2 },
          { setter: "setOffsetMonths", value: -1 },
          { setter: "setOffsetDays", value: 5 },
          { setter: "setOffsetHours", value: -3 },
          { setter: "setOffsetMinutes", value: 15 },
          { setter: "setOffsetSeconds", value: -30 }
        ],
        expected: { years: 2, months: -1, days: 5, hours: -3, minutes: 15, seconds: -30 }
      }
    ])("returns $scenario", ({ setup, expected }) => {
      const { result } = renderHook(() => useOffsetState());

      act(() => {
        for (const { setter, value } of setup) {
          (
            result.current[setter as keyof typeof result.current] as unknown as (
              value: number
            ) => void
          )(value);
        }
      });

      const offset = result.current.getOffset();

      expect(offset).toEqual(expected);
    });

    describe.each([
      {
        field: "years",
        setter: "setOffsetYears",
        value: 1,
        expected: { years: 1, months: 0, days: 0, hours: 0, minutes: 0, seconds: 0 }
      },
      {
        field: "months",
        setter: "setOffsetMonths",
        value: 5,
        expected: { years: 0, months: 5, days: 0, hours: 0, minutes: 0, seconds: 0 }
      },
      {
        field: "days",
        setter: "setOffsetDays",
        value: 15,
        expected: { years: 0, months: 0, days: 15, hours: 0, minutes: 0, seconds: 0 }
      },
      {
        field: "hours",
        setter: "setOffsetHours",
        value: 12,
        expected: { years: 0, months: 0, days: 0, hours: 12, minutes: 0, seconds: 0 }
      },
      {
        field: "minutes",
        setter: "setOffsetMinutes",
        value: 30,
        expected: { years: 0, months: 0, days: 0, hours: 0, minutes: 30, seconds: 0 }
      },
      {
        field: "seconds",
        setter: "setOffsetSeconds",
        value: 45,
        expected: { years: 0, months: 0, days: 0, hours: 0, minutes: 0, seconds: 45 }
      }
    ])("returns correct offset when only $field is set", ({ setter, value, expected }) => {
      const { result } = renderHook(() => useOffsetState());

      act(() => {
        (
          result.current[setter as keyof typeof result.current] as unknown as (
            value: number
          ) => void
        )(value);
      });

      const offset = result.current.getOffset();

      expect(offset).toEqual(expected);
    });

    it("returns offset object with correct structure", () => {
      const { result } = renderHook(() => useOffsetState());

      const offset = result.current.getOffset();

      expect(offset).toHaveProperty("years");
      expect(offset).toHaveProperty("months");
      expect(offset).toHaveProperty("days");
      expect(offset).toHaveProperty("hours");
      expect(offset).toHaveProperty("minutes");
      expect(offset).toHaveProperty("seconds");
    });

    it("returns updated offset after calling reset", () => {
      const { result } = renderHook(() => useOffsetState());

      act(() => {
        result.current.setOffsetYears(5);
        result.current.setOffsetMonths(6);
      });

      act(() => {
        result.current.reset();
      });

      const offset = result.current.getOffset();

      expect(offset).toEqual({
        years: 0,
        months: 0,
        days: 0,
        hours: 0,
        minutes: 0,
        seconds: 0
      });
    });
  });
});
