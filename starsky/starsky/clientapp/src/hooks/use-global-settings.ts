import { SupportedLanguages } from "../shared/language";

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
        return SupportedLanguages.nl;
      case "nl-nl":
        return SupportedLanguages.nl;
      case "nl":
        return SupportedLanguages.nl;
      case "de":
        return SupportedLanguages.de;
      case "de-de":
        return SupportedLanguages.de;
      case "de-at":
        return SupportedLanguages.de;
      case "de-be":
        return SupportedLanguages.de;
      case "de-ch":
        return SupportedLanguages.de;
      case "de-it":
        return SupportedLanguages.de;
      case "de-li":
        return SupportedLanguages.de;
      case "de-lu":
        return SupportedLanguages.de;
      default:
        return SupportedLanguages.en;
    }
  };

  return { language: parseLanguage() };
};

export default useGlobalSettings;
