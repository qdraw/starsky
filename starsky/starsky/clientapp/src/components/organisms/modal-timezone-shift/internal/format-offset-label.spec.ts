import { formatOffsetLabel } from "./format-offset-label";

describe("formatOffsetLabel", () => {
  it("returns 'No shift' when all values are zero", () => {
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", 0, "Hours", 0, "Minutes", 0, "Seconds", 0)
    ).toBe("No shift");
  });

  it("formats positive values with plus sign", () => {
    expect(
      formatOffsetLabel("Years", 1, "Months", 0, "Days", 0, "Hours", 0, "Minutes", 0, "Seconds", 0)
    ).toBe("+1 Years");
    expect(
      formatOffsetLabel("Years", 0, "Months", 2, "Days", 0, "Hours", 0, "Minutes", 0, "Seconds", 0)
    ).toBe("+2 Months");
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", 3, "Hours", 0, "Minutes", 0, "Seconds", 0)
    ).toBe("+3 Days");
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", 0, "Hours", 4, "Minutes", 0, "Seconds", 0)
    ).toBe("+4 Hours");
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", 0, "Hours", 0, "Minutes", 5, "Seconds", 0)
    ).toBe("+5 Minutes");
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", 0, "Hours", 0, "Minutes", 0, "Seconds", 6)
    ).toBe("+6 Seconds");
  });

  it("formats negative values without plus sign", () => {
    expect(
      formatOffsetLabel("Years", -1, "Months", 0, "Days", 0, "Hours", 0, "Minutes", 0, "Seconds", 0)
    ).toBe("-1 Years");
    expect(
      formatOffsetLabel("Years", 0, "Months", -2, "Days", 0, "Hours", 0, "Minutes", 0, "Seconds", 0)
    ).toBe("-2 Months");
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", -3, "Hours", 0, "Minutes", 0, "Seconds", 0)
    ).toBe("-3 Days");
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", 0, "Hours", -4, "Minutes", 0, "Seconds", 0)
    ).toBe("-4 Hours");
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", 0, "Hours", 0, "Minutes", -5, "Seconds", 0)
    ).toBe("-5 Minutes");
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", 0, "Hours", 0, "Minutes", 0, "Seconds", -6)
    ).toBe("-6 Seconds");
  });

  it("formats multiple nonzero values, comma separated", () => {
    expect(
      formatOffsetLabel("Years", 1, "Months", 2, "Days", 0, "Hours", 0, "Minutes", 0, "Seconds", 0)
    ).toBe("+1 Years, +2 Months");
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", 3, "Hours", 4, "Minutes", 0, "Seconds", 0)
    ).toBe("+3 Days, +4 Hours");
    expect(
      formatOffsetLabel("Years", 0, "Months", 0, "Days", 0, "Hours", 0, "Minutes", 5, "Seconds", 6)
    ).toBe("+5 Minutes, +6 Seconds");
    expect(
      formatOffsetLabel("Years", -1, "Months", 0, "Days", 0, "Hours", 2, "Minutes", 0, "Seconds", 0)
    ).toBe("-1 Years, +2 Hours");
  });

  it("handles all values nonzero", () => {
    expect(
      formatOffsetLabel(
        "Years",
        1,
        "Months",
        -2,
        "Days",
        3,
        "Hours",
        -4,
        "Minutes",
        5,
        "Seconds",
        -6
      )
    ).toBe("+1 Years, -2 Months, +3 Days, -4 Hours, +5 Minutes, -6 Seconds");
  });
});
