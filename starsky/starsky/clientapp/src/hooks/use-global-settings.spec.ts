import { SupportedLanguages } from "../shared/language";
import { mountReactHook } from "./___tests___/test-hook";
import useGlobalSettings, { IGlobalSettings } from "./use-global-settings";

describe("useGlobalSettings", () => {
  describe("language", () => {
    let setupComponent;
    let hook: IGlobalSettings;

    function runHook() {
      setupComponent = mountReactHook(useGlobalSettings, []);
      hook = setupComponent.componentHook as IGlobalSettings;
    }

    it("get default language", () => {
      runHook();
      expect(hook.language).toBe(SupportedLanguages.en);
    });

    it("get dutch language nl", () => {
      const languageGetter = jest.spyOn(window.navigator, "language", "get");
      languageGetter.mockReturnValue("nl");

      runHook();

      expect(hook.language).toBe(SupportedLanguages.nl);
    });

    it("get dutch language nl-NL", () => {
      const languageGetter = jest.spyOn(window.navigator, "language", "get");
      languageGetter.mockReturnValue("nl-NL");

      runHook();

      expect(hook.language).toBe(SupportedLanguages.nl);
    });

    it("get dutch language nl-BE", () => {
      const languageGetter = jest.spyOn(window.navigator, "language", "get");
      languageGetter.mockReturnValue("nl-BE");

      runHook();

      expect(hook.language).toBe(SupportedLanguages.nl);
    });

    it("get german language de", () => {
      const languageGetter = jest.spyOn(window.navigator, "language", "get");
      languageGetter.mockReturnValue("de");

      runHook();

      expect(hook.language).toBe(SupportedLanguages.de);
    });

    it("get german language de-AT", () => {
      const languageGetter = jest.spyOn(window.navigator, "language", "get");
      languageGetter.mockReturnValue("de-AT");

      runHook();

      expect(hook.language).toBe(SupportedLanguages.de);
    });

    it("get german language de-BE", () => {
      const languageGetter = jest.spyOn(window.navigator, "language", "get");
      languageGetter.mockReturnValue("de-BE");

      runHook();

      expect(hook.language).toBe(SupportedLanguages.de);
    });

    it("get german language de-CH", () => {
      const languageGetter = jest.spyOn(window.navigator, "language", "get");
      languageGetter.mockReturnValue("de-CH");

      runHook();

      expect(hook.language).toBe(SupportedLanguages.de);
    });

    it("get german language de-IT", () => {
      const languageGetter = jest.spyOn(window.navigator, "language", "get");
      languageGetter.mockReturnValue("de-IT");

      runHook();

      expect(hook.language).toBe(SupportedLanguages.de);
    });

    it("get german language de-LI", () => {
      const languageGetter = jest.spyOn(window.navigator, "language", "get");
      languageGetter.mockReturnValue("de-LI");

      runHook();

      expect(hook.language).toBe(SupportedLanguages.de);
    });

    it("get german language de-LU", () => {
      const languageGetter = jest.spyOn(window.navigator, "language", "get");
      languageGetter.mockReturnValue("de-LU");

      runHook();

      expect(hook.language).toBe(SupportedLanguages.de);
    });
  });
});
