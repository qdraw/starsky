export enum SupportedLanguages {
  nl = "nl",
  en = "en"
}

export class Language {
  /**
   *
   */
  constructor(selectedLanguage: SupportedLanguages) {
    this.selectedLanguage = selectedLanguage;
  }

  private selectedLanguage: SupportedLanguages;

  /**
   * WIP
   * @param key
   * @returns
   */
  public key(content: { en: string; nl: string }): string {
    return this.text(content.nl, content.en);
  }

  /**
   * Get the right content based on the language
   * Map used to be Map<any,string> and  nl = "nl" as any
   */
  public text(nl: string, en: string): string {
    const selectedLanguageMap = new Map<SupportedLanguages, string>([
      [SupportedLanguages.nl, nl],
      [SupportedLanguages.en, en]
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
      text = text.replace(new RegExp(token[index]), dynamicValue[index]);
    }
    return text;
  }
}
