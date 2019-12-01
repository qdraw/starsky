import { isValidDate, leftPad, parseRelativeDate } from './date';

describe("date", () => {
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
      var result = parseRelativeDate(undefined);
      expect(result).toBe("");
    });

    it("random", () => {
      var result = parseRelativeDate("dd");
      expect(result).toBe("");
    });

    it("non valid date", () => {
      var result = parseRelativeDate("2019-02-40T01:00:00+00:00");
      expect(result).toBe("");
    });

    it("yesterday", () => {
      var yesterdayDate = new Date();
      // to get 24 hours ago
      yesterdayDate.setDate(yesterdayDate.getDate() - 1);

      var yesterday = `${yesterdayDate.getFullYear()}-${yesterdayDate.getMonth() + 1}-${yesterdayDate.getDate()} ${leftPad(yesterdayDate.getHours())}:${leftPad(yesterdayDate.getMinutes())}:${leftPad(yesterdayDate.getSeconds())}`;

      var result = parseRelativeDate(yesterday);
      expect(result).toBe("24 uur");
    });

    it("less than a hour", () => {
      var tenMinutesStamp = new Date(new Date().getTime() - (36 * 60000));
      var tenMinutes = `${tenMinutesStamp.getFullYear()}-${tenMinutesStamp.getMonth() + 1}-${tenMinutesStamp.getDate()} ${leftPad(tenMinutesStamp.getHours())}:${leftPad(tenMinutesStamp.getMinutes())}:${leftPad(tenMinutesStamp.getSeconds())}`

      var result = parseRelativeDate(tenMinutes);
      expect(result).toBe("36 minuten");
    });

    it("day before yesterday", () => {

      var dayBeforeYesterdayDate = new Date();
      // to get 48 hours ago
      dayBeforeYesterdayDate.setDate(dayBeforeYesterdayDate.getDate() - 2);

      var dayBeforeYesterday = `${dayBeforeYesterdayDate.getFullYear()}-${dayBeforeYesterdayDate.getMonth() + 1}-${dayBeforeYesterdayDate.getDate()}T${leftPad(dayBeforeYesterdayDate.getHours())}:${leftPad(dayBeforeYesterdayDate.getMinutes())}:${leftPad(dayBeforeYesterdayDate.getSeconds())}`;

      var result = parseRelativeDate(dayBeforeYesterday);
      expect(result).toBe(`${dayBeforeYesterdayDate.getDate()}-${dayBeforeYesterdayDate.getMonth() + 1}-${dayBeforeYesterdayDate.getFullYear()}`);
    });
  });
});