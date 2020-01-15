import { SupportedLanguages } from '../shared/language';
import useGlobalSettings, { IGlobalSettings } from './use-global-settings';
import { mountReactHook } from './___tests___/test-hook';


describe("useGlobalSettings", () => {

  describe("language", () => {

    let setupComponent;
    let hook: IGlobalSettings;

    it("get default language", () => {
      setupComponent = mountReactHook(useGlobalSettings, []);
      hook = setupComponent.componentHook as IGlobalSettings;

      expect(hook.language).toBe(SupportedLanguages.en)
    });

    it("get dutch language nl", () => {

      var languageGetter = jest.spyOn(window.navigator, 'language', 'get')
      languageGetter.mockReturnValue('nl');

      setupComponent = mountReactHook(useGlobalSettings, []);
      hook = setupComponent.componentHook as IGlobalSettings;

      expect(hook.language).toBe(SupportedLanguages.nl)
    });


    it("get dutch language NL-nl", () => {

      var languageGetter = jest.spyOn(window.navigator, 'language', 'get')
      languageGetter.mockReturnValue('NL-nl');

      setupComponent = mountReactHook(useGlobalSettings, []);
      hook = setupComponent.componentHook as IGlobalSettings;

      expect(hook.language).toBe(SupportedLanguages.nl)
    });

    it("get dutch language nl-BE", () => {

      var languageGetter = jest.spyOn(window.navigator, 'language', 'get')
      languageGetter.mockReturnValue('nl-BE');

      setupComponent = mountReactHook(useGlobalSettings, []);
      hook = setupComponent.componentHook as IGlobalSettings;

      expect(hook.language).toBe(SupportedLanguages.nl)
    });


  });

});