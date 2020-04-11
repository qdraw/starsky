import { SupportedLanguages } from '../shared/language';

export interface IGlobalSettings {
  language: SupportedLanguages;
}

const useGlobalSettings = (): IGlobalSettings => {

  /**
   * Parse Language
   */
  const parseLanguage = (): SupportedLanguages => {
    switch (navigator.language.toLowerCase()) {
      case "nl-be":
        return SupportedLanguages.nl
      case "nl-nl":
        return SupportedLanguages.nl
      case "nl":
        return SupportedLanguages.nl
      default:
        return SupportedLanguages.en;
    }
  }

  return { language: parseLanguage() };
};

export default useGlobalSettings;