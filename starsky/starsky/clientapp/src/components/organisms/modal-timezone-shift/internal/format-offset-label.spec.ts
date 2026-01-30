import { formatOffsetLabel } from "./format-offset-label";

describe("formatOffsetLabel", () => {
  it("returns 'No shift' when all values are zero", () => {
    expect(formatOffsetLabel(0, 0, 0, 0, 0, 0)).toBe("No shift");
  });

  it("formats positive values with plus sign", () => {
    expect(formatOffsetLabel(1, 0, 0, 0, 0, 0)).toBe("+1 years");
    expect(formatOffsetLabel(0, 2, 0, 0, 0, 0)).toBe("+2 months");
    expect(formatOffsetLabel(0, 0, 3, 0, 0, 0)).toBe("+3 days");
    expect(formatOffsetLabel(0, 0, 0, 4, 0, 0)).toBe("+4 hours");
    expect(formatOffsetLabel(0, 0, 0, 0, 5, 0)).toBe("+5 minutes");
    expect(formatOffsetLabel(0, 0, 0, 0, 0, 6)).toBe("+6 seconds");
  });

  it("formats negative values without plus sign", () => {
    expect(formatOffsetLabel(-1, 0, 0, 0, 0, 0)).toBe("-1 years");
    expect(formatOffsetLabel(0, -2, 0, 0, 0, 0)).toBe("-2 months");
    expect(formatOffsetLabel(0, 0, -3, 0, 0, 0)).toBe("-3 days");
    expect(formatOffsetLabel(0, 0, 0, -4, 0, 0)).toBe("-4 hours");
    expect(formatOffsetLabel(0, 0, 0, 0, -5, 0)).toBe("-5 minutes");
    expect(formatOffsetLabel(0, 0, 0, 0, 0, -6)).toBe("-6 seconds");
  });

  it("formats multiple nonzero values, comma separated", () => {
    expect(formatOffsetLabel(1, 2, 0, 0, 0, 0)).toBe("+1 years, +2 months");
    expect(formatOffsetLabel(0, 0, 3, 4, 0, 0)).toBe("+3 days, +4 hours");
    expect(formatOffsetLabel(0, 0, 0, 0, 5, 6)).toBe("+5 minutes, +6 seconds");
    expect(formatOffsetLabel(-1, 0, 0, 2, 0, 0)).toBe("-1 years, +2 hours");
  });

  it("handles all values nonzero", () => {
    expect(formatOffsetLabel(1, -2, 3, -4, 5, -6)).toBe(
      "+1 years, -2 months, +3 days, -4 hours, +5 minutes, -6 seconds"
    );
  });
});
