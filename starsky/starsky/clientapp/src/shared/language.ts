import { ILanguageLocalization } from "../interfaces/ILanguageLocalization";

export enum SupportedLanguages {
  nl = "nl",
  en = "en",
  de = "de"
}

export class Language {
  /**
   *
   */
  constructor(selectedLanguage: SupportedLanguages) {
    this.selectedLanguage = selectedLanguage;
  }

  private readonly selectedLanguage: SupportedLanguages;

  /**
   * WIP
   * @returns
   * @param content
   */
  public key(content: ILanguageLocalization, token?: string[], dynamicValue?: string[]): string {
    const text = this.text(content.nl, content.en, content.de);
    if (!token || !dynamicValue) {
      return text;
    }
    return this.token(text, token, dynamicValue);
  }

  /**
   * Get the right content based on the language
   * Map used to be Map<any,string> and  nl = "nl" as any
   */
  public text(nl: string, en: string, de: string): string {
    const selectedLanguageMap = new Map<SupportedLanguages, string>([
      [SupportedLanguages.nl, nl],
      [SupportedLanguages.en, en],
      [SupportedLanguages.de, de]
    ]);

    const content = selectedLanguageMap.get(this.selectedLanguage);
    return content ?? "";
  }

  /**
   * Replace token content
   * @param text string with a token
   * @param token what is the toke {test}
   * @param dynamicValue the value that is used
   */
  public token(text: string, token: string[], dynamicValue: string[]): string {
    for (let index = 0; index < token.length; index++) {
      if (dynamicValue[index]) {
        text = text.replace(new RegExp(token[index]), dynamicValue[index]);
      } else {
        text = text.replace(new RegExp(token[index]), "");
      }
    }
    return text;
  }
}
