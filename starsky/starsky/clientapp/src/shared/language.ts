export enum SupportedLanguages {
  nl = "nl" as any,
  en = "en" as any,
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
   * Get the right content based on the language
   */
  public text(nl: string, en: string): string {
    var selectedLanguageMap = new Map<number, string>([
      [SupportedLanguages.nl, nl],
      [SupportedLanguages.en, en],
    ]);

    var content = selectedLanguageMap.get(this.selectedLanguage);
    return content ? content : "";
  }

  /**
   * Replace token content
   * @param input string with a token
   * @param token what is the toke {test}
   * @param dynamicValue the value that is used
   */
  public token(input: string, token: string[], dynamicValue: string[]): string {
    return input.replace(token[0], dynamicValue[0])
  }

}