import { SupportedLanguages } from "../shared/language";
import useGlobalSettings, { IGlobalSettings } from "./use-global-settings";
import { mountReactHook } from "./___tests___/test-hook";

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
  });
});
