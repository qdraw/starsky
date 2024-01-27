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
  SecondsToHours
} from "./date";
import { SupportedLanguages } from "./language";

describe("date", () => {
  describe("parseDate", () => {
    it("undefined", () => {
      const result = parseDate("", SupportedLanguages.nl);
      expect(result).toBeFalsy();
    });

    it("utc time (ends with Z)", () => {
      const result = parseDate("2020-04-28T10:44:11Z", SupportedLanguages.nl);
      //                   NOT Invalid!
      expect(result).not.toBe("Invalid Date");
    });
    it("wrong format", () => {
      const result = parseDate("2020-30", SupportedLanguages.nl);
      expect(result).toBe("Invalid Date");
    });

    it("right formatted (nl)", () => {
      const result = parseDate("2020-01-01T01:01:01", SupportedLanguages.nl);
      expect(result).toContain("2020");
    });
  });

  describe("parseTime", () => {
    it("undefined", () => {
      const result = parseTime("");
      expect(result).toBeFalsy();
    });

    it("wrong format", () => {
      const result = parseTime("2020-30");
      expect(result).toBe("");
    });

    it("right formatted (nl)", () => {
      const result = parseTime("2020-01-01T01:01:01");
      expect(result).toBe("01:01:01");
    });

    it("right formatted summer time (nl)", () => {
      const result = parseTime("2020-04-10T23:40:33");
      expect(result).toBe("23:40:33");
    });
  });

  describe("parseTimeHour", () => {
    it("undefined", () => {
      const result = parseTimeHour("");
      expect(result).toBe(1);
    });

    it("wrong format", () => {
      const result = parseTimeHour("2020-30");
      expect(result).toBe(1);
    });

    it("right formatted (nl)", () => {
      const result = parseTimeHour("2020-01-01T01:01:01");
      expect(result).toBe(1);
    });

    it("right formatted summer time (nl)", () => {
      const result = parseTimeHour("2020-04-10T23:40:33");
      expect(result).toBe(23);
    });
  });

  describe("parseDateDate", () => {
    it("undefined", () => {
      const result = parseDateDate("");
      expect(result).toBe(1);
    });

    it("wrong format", () => {
      const result = parseDateDate("2020-30");
      expect(result).toBe(1);
    });

    it("right formatted (nl)", () => {
      const result = parseDateDate("2020-01-01T01:01:01");
      expect(result).toBe(1);
    });

    it("right formatted summer time (nl)", () => {
      const result = parseDateDate("2020-04-10T23:40:33");
      expect(result).toBe(10);
    });
  });

  describe("parseDateMonth", () => {
    it("undefined", () => {
      const result = parseDateMonth("");
      expect(result).toBe(1);
    });

    it("wrong format", () => {
      const result = parseDateMonth("2020-30");
      expect(result).toBe(1);
    });

    it("right formatted (nl)", () => {
      const result = parseDateMonth("2020-01-01T01:01:01");
      expect(result).toBe(1);
    });

    it("right formatted summer time (nl)", () => {
      const result = parseDateMonth("2020-12-10T23:40:33");
      expect(result).toBe(12);
    });
  });

  describe("parseDateYear", () => {
    it("undefined", () => {
      const result = parseDateYear("");
      expect(result).toBe(1);
    });

    it("wrong format", () => {
      const result = parseDateYear("2020-30");
      expect(result).toBe(1);
    });

    it("right formatted (nl)", () => {
      const result = parseDateYear("2020-01-01T01:01:01");
      expect(result).toBe(2020);
    });

    it("right formatted summer time (nl)", () => {
      const result = parseDateYear("2020-12-10T23:40:33");
      expect(result).toBe(2020);
    });
  });

  describe("isValidDate", () => {
    it("undefined", () => {
      const result = isValidDate(undefined);
      expect(result).toBeFalsy();
    });

    it("YYYY-MM-DD", () => {
      const result = isValidDate("2019-10-12");
      expect(result).toBeTruthy();
    });

    it("YYYY-MM-DD hh:mm:ss", () => {
      const result = isValidDate("2019-10-12 14:12:00");
      expect(result).toBeTruthy();
    });
  });

  describe("parseRelativeDate", () => {
    it("undefined", () => {
      const result = parseRelativeDate(undefined, SupportedLanguages.en);
      expect(result).toBe("");
    });

    it("random", () => {
      const result = parseRelativeDate("dd", SupportedLanguages.en);
      expect(result).toBe("");
    });

    it("non valid date", () => {
      const result = parseRelativeDate(
        "2019-02-40T01:00:00+00:00",
        SupportedLanguages.en
      );
      expect(result).toBe("");
    });

    it("yesterday", () => {
      const yesterdayDate = new Date();

      // to get 24 hours ago
      yesterdayDate.setDate(yesterdayDate.getDate() - 1);

      const yesterday =
        `${yesterdayDate.getFullYear()}-${leftPad(
          yesterdayDate.getMonth() + 1
        )}-` +
        `${leftPad(yesterdayDate.getDate())} ${leftPad(
          yesterdayDate.getHours()
        )}:${leftPad(yesterdayDate.getMinutes())}:` +
        `${leftPad(yesterdayDate.getSeconds())}`;

      const result = parseRelativeDate(yesterday, SupportedLanguages.en);

      // on the sunday that the timezone change i.e. March 29, 2020 (Europe DST) or 25 okt 2020
      if (
        new Date().getTimezoneOffset() !== yesterdayDate.getTimezoneOffset()
      ) {
        console.log("this unit test does not work today");
        return;
      }

      expect(result).toBe("24 {hour}");
    });

    it("less than a hour", () => {
      const tenMinutesStamp = new Date(new Date().getTime() - 36 * 60000);
      const tenMinutes = `${tenMinutesStamp.getFullYear()}-${
        tenMinutesStamp.getMonth() + 1
      }-
      ${tenMinutesStamp.getDate()} ${leftPad(tenMinutesStamp.getHours())}:
      ${leftPad(tenMinutesStamp.getMinutes())}:${leftPad(
        tenMinutesStamp.getSeconds()
      )}`;

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

      const result = parseRelativeDate(
        dayBeforeYesterday,
        SupportedLanguages.en
      );

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
    it("3:01", () => {
      const result = SecondsToHours(60 * 3 + 1);
      expect(result).toBe("3:01"); // 3 minutes and one second
    });
    it("3:11", () => {
      const result = SecondsToHours(60 * 3 + 11);
      expect(result).toBe("3:11"); // 3 minutes and 11 seconds
    });
    it("1:00:00", () => {
      const result = SecondsToHours(3600);
      expect(result).toBe("1:00:00"); // 1 hour
    });
    it("NaN", () => {
      const result = SecondsToHours(NaN);
      expect(result).toBe("0:00"); // return 0 when its NaN
    });
  });
});
