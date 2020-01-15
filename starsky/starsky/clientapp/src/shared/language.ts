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
   * name
   */
  public text(nl: string, en: string): string {
    var selectedLanguageMap = new Map<number, string>([
      [SupportedLanguages.nl, nl],
      [SupportedLanguages.en, en],
    ]);

    var content = selectedLanguageMap.get(this.selectedLanguage);
    return content ? content : "";
  }

}