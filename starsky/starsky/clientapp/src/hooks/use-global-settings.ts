import { SupportedLanguages } from '../shared/language';

export interface IGlobalSettings {
  language: SupportedLanguages;
}

const useGlobalSettings = (): IGlobalSettings => {

  /**
   * Parse Language
   */
  const parseLanguage = (): SupportedLanguages => {

    var language: SupportedLanguages;

    switch (navigator.language) {
      case "nl-BE":
        language = SupportedLanguages.nl
        break;
      case "NL-nl":
        language = SupportedLanguages.nl
        break;
      case "nl":
        language = SupportedLanguages.nl
        break;
      default:
        language = SupportedLanguages.en;
        break;
    }

    return language;
  }


  return { language: parseLanguage() };
};

export default useGlobalSettings;