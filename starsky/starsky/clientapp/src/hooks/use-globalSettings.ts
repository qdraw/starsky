import { SupportedLanguages } from '../shared/language';

export interface IGlobalSettings {
  language: SupportedLanguages;
}

const useGlobalSettings = (): IGlobalSettings => {
  var language: SupportedLanguages = navigator.language === SupportedLanguages.nl.toString() ? SupportedLanguages.nl : SupportedLanguages.en;

  return { language };
};

export default useGlobalSettings;