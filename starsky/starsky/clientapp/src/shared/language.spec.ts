import { Language, SupportedLanguages } from "./language";

describe("keyboard", () => {
  const language = new Language(SupportedLanguages.nl);

  describe("text", () => {
    it("get different content (dutch)", () => {
      const result = language.text("dutch", "english", "deutsch");
      expect(result).toBe("dutch");
    });
    it("get different content (english)", () => {
      const result = new Language(SupportedLanguages.en).text("dutch", "english", "deutsch");
      expect(result).toBe("english");
    });
  });

  describe("key", () => {
    it("get different content (dutch)", () => {
      const result = language.key({
        nl: "dutch",
        en: "english",
        de: "deutsch"
      });
      expect(result).toBe("dutch");
    });
    it("get different content (english)", () => {
      const result = new Language(SupportedLanguages.en).key({
        nl: "dutch",
        en: "english",
        de: "deutsch"
      });
      expect(result).toBe("english");
    });

    it("replace keys - english", () => {
      const data = {
        nl: "Het onderstaande veld mag maximaal {maxlength} tekens hebben",
        en: "The field below can have a maximum of {maxlength} characters",
        de: "Das Feld unten kann maximal {maxlength} Zeichen enthalten"
      };
      const maxlength = 14;

      const result = new Language(SupportedLanguages.en).key(
        data,
        ["{maxlength}"],
        [maxlength.toString()]
      );
      expect(result).toBe("The field below can have a maximum of 14 characters");
    });

    it("replace keys - german", () => {
      const data = {
        nl: "Het onderstaande veld mag maximaal {maxlength} tekens hebben",
        en: "The field below can have a maximum of {maxlength} characters",
        de: "Das Feld unten kann maximal {maxlength} Zeichen enthalten"
      };
      const maxlength = 14;

      const result = new Language(SupportedLanguages.de).key(
        data,
        ["{maxlength}"],
        [maxlength.toString()]
      );
      expect(result).toBe("Das Feld unten kann maximal 14 Zeichen enthalten");
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
