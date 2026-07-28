import {
  isValidDate,
  leftPad,
  parseDate,
  parseDateDate,
  parseDateMonth,
  parseDateYear,
  parseRelativeDate,
  parseTime,
  parseTimeHour,
  parseTimeMinute,
  parseTimeSeconds,
  SecondsToHours
} from "./date";
import { SupportedLanguages } from "./language";

describe("date", () => {
  describe("parseDate", () => {
    it.each([
      {
        description: "undefined",
        input: "",
        assertion: (result: string) => expect(result).toBeFalsy()
      },
      {
        description: "utc time (ends with Z)",
        input: "2020-04-28T10:44:11Z",
        assertion: (result: string) => expect(result).not.toBe("Invalid Date")
      },
      {
        description: "Timezone time",
        input: "2020-04-28T10:44:43.123456+01:00",
        assertion: (result: string) => expect(result).toBe("dinsdag 28 april 2020")
      },
      {
        description: "wrong format",
        input: "2020-30",
        assertion: (result: string) => expect(result).toBe("Invalid Date")
      },
      {
        description: "right formatted (nl)",
        input: "2020-01-01T01:01:01",
        assertion: (result: string) => expect(result).toContain("2020")
      }
    ])("$description", ({ input, assertion }) => {
      const result = parseDate(input, SupportedLanguages.nl);
      assertion(result);
    });
  });

  describe("parseTime", () => {
    it.each([
      { description: "undefined", input: "", expected: "" },
      { description: "wrong format", input: "2020-30", expected: "" },
      { description: "right formatted (nl)", input: "2020-01-01T01:01:01", expected: "01:01:01" },
      {
        description: "Timezone time (parseTime)",
        input: "2020-04-28T10:44:43.123456+01:00",
        expected: "09:44:43"
      },
      {
        description: "right formatted summer time (nl)",
        input: "2020-04-10T23:40:33",
        expected: "23:40:33"
      }
    ])("$description", ({ input, expected }) => {
      const result = parseTime(input);
      expect(result).toBe(expected);
    });
  });

  describe("parseTimeHour", () => {
    it.each([
      { description: "undefined", input: "", expected: undefined },
      { description: "wrong format", input: "2020-30", expected: undefined },
      { description: "right formatted (nl)", input: "2020-01-01T01:01:01", expected: 1 },
      {
        description: "Timezone time (parseTimeHour)",
        input: "2020-04-28T10:44:43.123456+01:00",
        expected: 9
      },
      {
        description: "right formatted summer time (nl)",
        input: "2020-04-10T23:40:33",
        expected: 23
      }
    ])("$description", ({ input, expected }) => {
      const result = parseTimeHour(input);
      expect(result).toBe(expected);
    });
  });

  describe("parseTimeMinute", () => {
    it.each([
      { description: "undefined", input: "", expected: undefined },
      { description: "wrong format", input: "2020-30", expected: undefined },
      { description: "right formatted (nl)", input: "2020-01-01T01:01:01", expected: 1 },
      {
        description: "Timezone time (parseTimeSeconds)",
        input: "2020-04-28T10:44:43.123456+01:00",
        expected: 44
      },
      {
        description: "right formatted summer time (nl)",
        input: "2020-04-10T23:40:33",
        expected: 40
      }
    ])("$description", ({ input, expected }) => {
      const result = parseTimeMinute(input);
      expect(result).toBe(expected);
    });
  });

  describe("parseTimeSeconds", () => {
    it.each([
      { description: "undefined", input: "", expected: undefined },
      { description: "wrong format", input: "2020-30", expected: undefined },
      { description: "right formatted (nl)", input: "2020-01-01T01:01:01", expected: 1 },
      {
        description: "Timezone time (parseTimeSeconds)",
        input: "2020-04-28T10:44:43.123456+01:00",
        expected: 43
      },
      {
        description: "right formatted summer time (nl)",
        input: "2020-04-10T23:40:33",
        expected: 33
      }
    ])("$description", ({ input, expected }) => {
      const result = parseTimeSeconds(input);
      expect(result).toBe(expected);
    });
  });

  describe("parseDateDate", () => {
    it.each([
      { description: "undefined", input: "", expected: undefined },
      { description: "wrong format", input: "2020-30", expected: undefined },
      { description: "right formatted (nl)", input: "2020-01-01T01:01:01", expected: 1 },
      {
        description: "Timezone time (parseDateDate)",
        input: "2020-04-28T10:44:43.123456+01:00",
        expected: 28
      },
      {
        description: "right formatted summer time (nl)",
        input: "2020-04-10T23:40:33",
        expected: 10
      }
    ])("$description", ({ input, expected }) => {
      const result = parseDateDate(input);
      expect(result).toBe(expected);
    });
  });

  describe("parseDateMonth", () => {
    it.each([
      { description: "undefined", input: "", expected: undefined },
      { description: "wrong format", input: "2020-30", expected: undefined },
      { description: "right formatted (nl)", input: "2020-01-01T01:01:01", expected: 1 },
      {
        description: "Timezone time (parseDateMonth)",
        input: "2020-04-28T10:44:43.123456+01:00",
        expected: 4
      },
      {
        description: "right formatted summer time (nl)",
        input: "2020-12-10T23:40:33",
        expected: 12
      }
    ])("$description", ({ input, expected }) => {
      const result = parseDateMonth(input);
      expect(result).toBe(expected);
    });
  });

  describe("parseDateYear", () => {
    it.each([
      { description: "undefined", input: "", expected: undefined },
      { description: "wrong format", input: "2020-30", expected: undefined },
      { description: "right formatted (nl)", input: "2020-01-01T01:01:01", expected: 2020 },
      {
        description: "Timezone time (parseDateYear)",
        input: "2020-04-28T10:44:43.123456+01:00",
        expected: 2020
      },
      {
        description: "right formatted summer time (nl)",
        input: "2020-12-10T23:40:33",
        expected: 2020
      }
    ])("$description", ({ input, expected }) => {
      const result = parseDateYear(input);
      expect(result).toBe(expected);
    });
  });

  describe("isValidDate", () => {
    it.each([
      { description: "undefined", input: undefined, expected: false },
      { description: "YYYY-MM-DD", input: "2019-10-12", expected: true },
      { description: "YYYY-MM-DD hh:mm:ss", input: "2019-10-12 14:12:00", expected: true },
      {
        description: "Timezone time (isValidDate)",
        input: "2020-04-28T10:44:43.123456+01:00",
        expected: true
      }
    ])("$description", ({ input, expected }) => {
      const result = isValidDate(input);
      expect(result).toBe(expected);
    });
  });

  describe("parseRelativeDate", () => {
    it.each([
      {
        description: "undefined",
        input: undefined,
        assertion: (result: string) => expect(result).toBe("")
      },
      {
        description: "random",
        input: "dd",
        assertion: (result: string) => expect(result).toBe("")
      },
      {
        description: "non valid date",
        input: "2019-02-40T01:00:00+00:00",
        assertion: (result: string) => expect(result).toBe("")
      },
      {
        description: "Timezone time (isValidDate)",
        input: "2020-04-28T10:44:43.123456+01:00",
        assertion: (result: string) => {
          expect(result).toContain("Tuesday"); // with or without comma
          expect(result).toContain("28 April 2020");
        }
      }
    ])("$description", ({ input, assertion }) => {
      const result = parseRelativeDate(input, SupportedLanguages.en);
      assertion(result);
    });

    it("yesterday", () => {
      const yesterdayDate = new Date();

      // to get 24 hours ago
      yesterdayDate.setDate(yesterdayDate.getDate() - 1);

      const yesterday =
        `${yesterdayDate.getFullYear()}-${leftPad(yesterdayDate.getMonth() + 1)}-` +
        `${leftPad(yesterdayDate.getDate())} ${leftPad(
          yesterdayDate.getHours()
        )}:${leftPad(yesterdayDate.getMinutes())}:` +
        `${leftPad(yesterdayDate.getSeconds())}`;

      const result = parseRelativeDate(yesterday, SupportedLanguages.en);

      // on the sunday that the timezone change i.e. March 29, 2020 (Europe DST) or 25 okt 2020
      if (new Date().getTimezoneOffset() !== yesterdayDate.getTimezoneOffset()) {
        console.log("this unit test does not work today");
        return;
      }

      expect(result).toBe("24 {hour}");
    });

    it("less than a hour", () => {
      const tenMinutesStamp = new Date(new Date().getTime() - 36 * 60000);
      const tenMinutes = `${tenMinutesStamp.getFullYear()}-${tenMinutesStamp.getMonth() + 1}-
      ${tenMinutesStamp.getDate()} ${leftPad(tenMinutesStamp.getHours())}:
      ${leftPad(tenMinutesStamp.getMinutes())}:${leftPad(tenMinutesStamp.getSeconds())}`;

      const result = parseRelativeDate(tenMinutes, SupportedLanguages.en);
      expect(result).toBe("36 {minutes}");
    });

    it("day before yesterday", () => {
      const dayBeforeYesterdayDate = new Date();
      // to get 48 hours ago
      dayBeforeYesterdayDate.setDate(dayBeforeYesterdayDate.getDate() - 2);

      let dayBeforeYesterday = `${dayBeforeYesterdayDate.getFullYear()}-
        ${leftPad(dayBeforeYesterdayDate.getMonth() + 1)}-${leftPad(
          dayBeforeYesterdayDate.getDate()
        )}T
        ${leftPad(dayBeforeYesterdayDate.getHours())}:${leftPad(
          dayBeforeYesterdayDate.getMinutes()
        )}:
        ${leftPad(dayBeforeYesterdayDate.getSeconds())}`;

      // remove space and newlines from prev variable
      dayBeforeYesterday = dayBeforeYesterday.replace(/\s|\n|\r\n/gi, "");

      const result = parseRelativeDate(dayBeforeYesterday, SupportedLanguages.en);

      expect(result).toBe(
        dayBeforeYesterdayDate.toLocaleDateString("en-GB", {
          weekday: "long",
          year: "numeric",
          month: "long",
          day: "numeric"
        })
      );
    });
  });

  describe("SecondsToHours", () => {
    it.each([
      { description: "3:01", input: 60 * 3 + 1, expected: "3:01" },
      { description: "3:11", input: 60 * 3 + 11, expected: "3:11" },
      { description: "1:00:00", input: 3600, expected: "1:00:00" },
      { description: "NaN", input: NaN, expected: "0:00" }
    ])("$description", ({ input, expected }) => {
      const result = SecondsToHours(input);
      expect(result).toBe(expected);
    });
  });
});
