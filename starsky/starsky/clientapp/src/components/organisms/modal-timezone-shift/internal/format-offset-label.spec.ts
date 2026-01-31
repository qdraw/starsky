import { formatOffsetLabel } from "./format-offset-label";

describe("formatOffsetLabel", () => {
  it("returns 'No shift' when all values are zero", () => {
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("No shift");
  });

  it("formats positive values with plus sign", () => {
    expect(
      formatOffsetLabel(
        { label: "Years", value: 1 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("+1 Years");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 2 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("+2 Months");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: 3 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("+3 Days");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 4 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("+4 Hours");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 5 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("+5 Minutes");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 6 }
      )
    ).toBe("+6 Seconds");
  });

  it("formats negative values without plus sign", () => {
    expect(
      formatOffsetLabel(
        { label: "Years", value: -1 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("-1 Years");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: -2 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("-2 Months");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: -3 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("-3 Days");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: -4 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("-4 Hours");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: -5 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("-5 Minutes");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: -6 }
      )
    ).toBe("-6 Seconds");
  });

  it("formats multiple nonzero values, comma separated", () => {
    expect(
      formatOffsetLabel(
        { label: "Years", value: 1 },
        { label: "Months", value: 2 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("+1 Years, +2 Months");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: 3 },
        { label: "Hours", value: 4 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("+3 Days, +4 Hours");
    expect(
      formatOffsetLabel(
        { label: "Years", value: 0 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 0 },
        { label: "Minutes", value: 5 },
        { label: "Seconds", value: 6 }
      )
    ).toBe("+5 Minutes, +6 Seconds");
    expect(
      formatOffsetLabel(
        { label: "Years", value: -1 },
        { label: "Months", value: 0 },
        { label: "Days", value: 0 },
        { label: "Hours", value: 2 },
        { label: "Minutes", value: 0 },
        { label: "Seconds", value: 0 }
      )
    ).toBe("-1 Years, +2 Hours");
  });

  it("handles all values nonzero", () => {
    expect(
      formatOffsetLabel(
        { label: "Years", value: 1 },
        { label: "Months", value: -2 },
        { label: "Days", value: 3 },
        { label: "Hours", value: -4 },
        { label: "Minutes", value: 5 },
        { label: "Seconds", value: -6 }
      )
    ).toBe("+1 Years, -2 Months, +3 Days, -4 Hours, +5 Minutes, -6 Seconds");
  });
});
