import { isValidDate, leftPad, parseDate, parseRelativeDate, parseTime, secondsToHours } from './date';
import { SupportedLanguages } from './language';

describe("date", () => {

  describe("parseDate", () => {
    it("undefined", () => {
      var result = parseDate("", SupportedLanguages.nl);
      expect(result).toBeFalsy()
    });

    it("wrong format", () => {
      var result = parseDate("2020-30", SupportedLanguages.nl);
      expect(result).toBe("Invalid Date");
    });

    it("right formated (nl)", () => {
      var result = parseDate("2020-01-01T01:01:01", SupportedLanguages.nl);
      expect(result).toContain("2020");
    });

  });

  describe("parseTime", () => {
    it("undefined", () => {
      var result = parseTime("");
      expect(result).toBeFalsy()
    });

    it("wrong format", () => {
      var result = parseTime("2020-30");
      expect(result).toBe("::");
    });

    it("right formated (nl)", () => {
      var result = parseTime("2020-01-01T01:01:01");
      expect(result).toBe("01:01:01");
    });

  });

  describe("isValidDate", () => {
    it("undefined", () => {
      var result = isValidDate(undefined);
      expect(result).toBeFalsy()
    });

    it("YYYY-MM-DD", () => {
      var result = isValidDate("2019-10-12");
      expect(result).toBeTruthy()
    });

    it("YYYY-MM-DD hh:mm:ss", () => {
      var result = isValidDate("2019-10-12 14:12:00");
      expect(result).toBeTruthy()
    });
  });

  describe("parseRelativeDate", () => {
    it("undefined", () => {
      var result = parseRelativeDate(undefined, SupportedLanguages.en);
      expect(result).toBe("");
    });

    it("random", () => {
      var result = parseRelativeDate("dd", SupportedLanguages.en);
      expect(result).toBe("");
    });

    it("non valid date", () => {
      var result = parseRelativeDate("2019-02-40T01:00:00+00:00", SupportedLanguages.en);
      expect(result).toBe("");
    });

    it("yesterday", () => {
      var yesterdayDate = new Date();
      // to get 24 hours ago
      yesterdayDate.setDate(yesterdayDate.getDate() - 1);

      var yesterday = `${yesterdayDate.getFullYear()}-${yesterdayDate.getMonth() + 1}-
      ${yesterdayDate.getDate()} ${leftPad(yesterdayDate.getHours())}:${leftPad(yesterdayDate.getMinutes())}:
      ${leftPad(yesterdayDate.getSeconds())}`;

      var result = parseRelativeDate(yesterday, SupportedLanguages.en);
      expect(result).toBe("24 {hour}");
    });

    it("less than a hour", () => {
      var tenMinutesStamp = new Date(new Date().getTime() - (36 * 60000));
      var tenMinutes = `${tenMinutesStamp.getFullYear()}-${tenMinutesStamp.getMonth() + 1}-
      ${tenMinutesStamp.getDate()} ${leftPad(tenMinutesStamp.getHours())}:
      ${leftPad(tenMinutesStamp.getMinutes())}:${leftPad(tenMinutesStamp.getSeconds())}`

      var result = parseRelativeDate(tenMinutes, SupportedLanguages.en);
      expect(result).toBe("36 {minutes}");
    });

    it("day before yesterday", () => {

      var dayBeforeYesterdayDate = new Date();
      // to get 48 hours ago
      dayBeforeYesterdayDate.setDate(dayBeforeYesterdayDate.getDate() - 2);

      var dayBeforeYesterday = `${dayBeforeYesterdayDate.getFullYear()}-
        ${leftPad(dayBeforeYesterdayDate.getMonth() + 1)}-${leftPad(dayBeforeYesterdayDate.getDate())}T
        ${leftPad(dayBeforeYesterdayDate.getHours())}:${leftPad(dayBeforeYesterdayDate.getMinutes())}:
        ${leftPad(dayBeforeYesterdayDate.getSeconds())}`;

      // remove space and newlines from prev variable
      dayBeforeYesterday = dayBeforeYesterday.replace(/\s|\n|\r\n/ig, "");

      var result = parseRelativeDate(dayBeforeYesterday, SupportedLanguages.en);

      expect(result).toBe(dayBeforeYesterdayDate.toLocaleDateString("en-GB", {
        weekday: 'long',
        year: 'numeric', month: 'long', day: 'numeric'
      }));
    });
  });

  describe("secondsToHours", () => {
    it("3:01", () => {
      var result = secondsToHours(60 * 3 + 1);
      expect(result).toBe('3:01'); // 3 minutes and one second
    });

    it("3:11", () => {
      var result = secondsToHours(60 * 3 + 11);
      expect(result).toBe('3:11'); // 3 minutes and 11 seconds
    });
    it("1:00:00", () => {
      var result = secondsToHours(3600);
      expect(result).toBe('1:00:00'); // 1 hour
    });

  });

});