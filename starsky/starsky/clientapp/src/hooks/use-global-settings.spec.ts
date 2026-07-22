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

    it.each(["nl", "nl-NL", "nl-BE"])("get dutch language %s", (language) => {
      const languageGetter = jest.spyOn(globalThis.navigator, "language", "get");
      languageGetter.mockReturnValue(language);

      runHook();

      expect(hook.language).toBe(SupportedLanguages.nl);
    });

    it.each(["de", "de-DE", "de-AT", "de-BE", "de-CH", "de-IT", "de-LI", "de-LU"])(
      "get german language %s",
      (language) => {
        const languageGetter = jest.spyOn(globalThis.navigator, "language", "get");
        languageGetter.mockReturnValue(language);

        runHook();

        expect(hook.language).toBe(SupportedLanguages.de);
      }
    );
  });
});
