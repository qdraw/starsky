
import { Language, SupportedLanguages } from './language';

describe("keyboard", () => {
  var language = new Language(SupportedLanguages.nl);

  describe("text", () => {
    it("get different content (dutch)", () => {
      var result = language.text("dutch", "english");
      expect(result).toBe("dutch");
    });
    it("get different content (english)", () => {
      var result = new Language(SupportedLanguages.en).text("dutch", "english");
      expect(result).toBe("english");
    });
  });

  describe("token", () => {
    it("multiple tokens", () => {

      var result = language.token("{lessThan1Minute} {minutes} {hour}",
        ["{lessThan1Minute}", "{minutes}", "{hour}"],
        ["<1minute", "minutes", "hour"]);

      expect(result).toBe("<1minute minutes hour");
    });
  });

});
