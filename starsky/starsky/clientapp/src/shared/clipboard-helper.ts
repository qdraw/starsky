export interface IClipboardData {
  tags: string;
  description: string;
  title: string;
}

export class ClipboardHelper {
  /**
   * Name of the sessionStorage
   */
  private readonly clipBoardName = "starskyClipboardData";

  public Copy(
    tagsReference: React.RefObject<HTMLDivElement>,
    descriptionReference: React.RefObject<HTMLDivElement>,
    titleReference: React.RefObject<HTMLDivElement>
  ): boolean {
    if (!tagsReference.current || !descriptionReference.current || !titleReference.current) {
      return false;
    }

    const tags = tagsReference.current.innerText;
    const description = descriptionReference.current.innerText;
    const title = titleReference.current.innerText;

    sessionStorage.setItem(
      this.clipBoardName,
      JSON.stringify({
        tags,
        description,
        title
      } as IClipboardData)
    );
    return true;
  }

  public Read(): IClipboardData | null {
    let result = {};
    try {
      const resultString = sessionStorage.getItem(this.clipBoardName);
      result = JSON.parse(resultString ?? "");
    } catch {
      return null;
    }
    return result as IClipboardData;
  }

  /**
   * Paste values in callback
   * @param updateChange callback function
   */
  public Paste(updateChange: (items: [string, string][]) => void): boolean {
    if (!updateChange) {
      return false;
    }
    const readData = this.Read();

    if (!readData) {
      return false;
    }

    updateChange([
      ["tags", readData.tags],
      ["description", readData.description],
      ["title", readData.title]
    ]);

    return true;
  }

  /**
   * Paste values in callback
   * @param updateChange callback function
   */
  public async PasteAsync(
    updateChange: (items: [string, string][]) => Promise<string | boolean>
  ): Promise<boolean> {
    if (!updateChange) {
      return false;
    }
    const readData = this.Read();

    if (!readData) {
      return false;
    }

    await updateChange([
      ["tags", readData.tags],
      ["description", readData.description],
      ["title", readData.title]
    ]);

    return true;
  }
}
