import { LatLongRound } from "./lat-long-round";

describe("LatLongRound", () => {
  it("rounds positive latitude to 6 decimal places", () => {
    expect(LatLongRound(37.123456789)).toBe(37.123457);
  });

  it("rounds negative longitude to 6 decimal places", () => {
    expect(LatLongRound(-122.987654321)).toBe(-122.987654);
  });

  it("returns 0 for undefined input", () => {
    expect(LatLongRound(undefined)).toBe(0);
  });
});
