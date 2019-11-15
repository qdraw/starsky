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
      var yesterday = `${new Date().getFullYear()}-${new Date().getMonth() + 1}-${new Date().getDate() - 1} ${leftPad(new Date().getHours())}:${leftPad(new Date().getMinutes())}:${leftPad(new Date().getSeconds())}`;
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
      var yesterday = `${new Date().getFullYear()}-${new Date().getMonth() + 1}-${new Date().getDate() - 2}T${leftPad(new Date().getHours())}:${leftPad(new Date().getMinutes())}:${leftPad(new Date().getSeconds())}+00:00`;
      var result = parseRelativeDate(yesterday);
      expect(result).toBe(`${new Date().getDate() - 2}-${new Date().getMonth() + 1}-${new Date().getFullYear()}`);
    });
  });
});