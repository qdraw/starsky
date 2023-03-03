import { Language, SupportedLanguages } from "./language";

describe("keyboard", () => {
  const language = new Language(SupportedLanguages.nl);

  describe("text", () => {
    it("get different content (dutch)", () => {
      const result = language.text("dutch", "english");
      expect(result).toBe("dutch");
    });
    it("get different content (english)", () => {
      const result = new Language(SupportedLanguages.en).text(
        "dutch",
        "english"
      );
      expect(result).toBe("english");
    });
  });

  describe("key", () => {
    it("get different content (dutch)", () => {
      const result = language.key({
        nl: "dutch",
        en: "english"
      });
      expect(result).toBe("dutch");
    });
    it("get different content (english)", () => {
      const result = new Language(SupportedLanguages.en).key({
        nl: "dutch",
        en: "english"
      });
      expect(result).toBe("english");
    });
  });

  describe("token", () => {
    it("multiple tokens", () => {
      const result = language.token(
        "{lessThan1Minute} {minutes} {hour}",
        ["{lessThan1Minute}", "{minutes}", "{hour}"],
        ["<1minute", "minutes", "hour"]
      );

      expect(result).toBe("<1minute minutes hour");
    });
  });
});
