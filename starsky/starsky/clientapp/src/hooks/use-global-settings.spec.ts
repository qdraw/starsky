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

    it.each([
      { language: "en-US", expected: SupportedLanguages.en },
      { language: "nl", expected: SupportedLanguages.nl },
      { language: "nl-NL", expected: SupportedLanguages.nl },
      { language: "nl-BE", expected: SupportedLanguages.nl },
      { language: "de", expected: SupportedLanguages.de },
      { language: "de-DE", expected: SupportedLanguages.de },
      { language: "de-AT", expected: SupportedLanguages.de },
      { language: "de-BE", expected: SupportedLanguages.de },
      { language: "de-CH", expected: SupportedLanguages.de },
      { language: "de-IT", expected: SupportedLanguages.de },
      { language: "de-LI", expected: SupportedLanguages.de },
      { language: "de-LU", expected: SupportedLanguages.de }
    ])("get language $language", ({ language, expected }) => {
      const languageGetter = jest.spyOn(globalThis.navigator, "language", "get");
      languageGetter.mockReturnValue(language);

      runHook();

      expect(hook.language).toBe(expected);
    });
  });
});
