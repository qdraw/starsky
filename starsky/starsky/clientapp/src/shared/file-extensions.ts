export class FileExtensions {
  /**
   * Match two filenames, make sure those are having the same extensions
   * @param from input.jpg
   * @param to changeto.jpg
   */
  public MatchExtension(from: string, to: string): boolean | null {
    const extensionRegex = /\.[0-9a-z]+$/;

    const fromExtMatchArray = extensionRegex.exec(from);
    if (!fromExtMatchArray) return null;

    const toExtMatchArray = extensionRegex.exec(to);
    if (!toExtMatchArray) return false;

    return toExtMatchArray[0] === fromExtMatchArray[0];
  }

  /**
   * Checks if the filename is valid
   * @param filename
   */
  public IsValidFileName(filename: string): boolean {
    const extensionRegex = /^\w(?:[a-zA-Z0-9 ._-]*[a-zA-Z0-9])?\.[a-zA-Z0-9_-]+$/;
    const fromExtMatchArray = extensionRegex.exec(filename);
    return !!fromExtMatchArray;
  }

  /**
   * Checks if the directory name is valid
   * @param directoryName only the name, not the full path
   */
  public IsValidDirectoryName(directoryName: string): boolean {
    const extensionRegex = /^[$a-zA-Z0-9_\s-]{2,}$/;
    const fromDirMatchArray = extensionRegex.exec(directoryName);
    return !!fromDirMatchArray;
  }

  /**
   * Get the parent path from your string
   * @param filePath filepath
   * @see https://stackoverflow.com/a/1130024
   */
  public GetParentPath(filePath: string) {
    if (!filePath) return "/";

    const parentRegex = /.+(?=\/[^/]+$)/;

    // Remove slash from end
    if (filePath.length >= 2 && filePath.endsWith("/")) {
      filePath = filePath.slice(0, -1);
    }

    const parentMatchArray = parentRegex.exec(filePath);

    if (!parentMatchArray) return "/";

    return parentMatchArray[0];
  }

  /**
   * extract fileName from string
   * @param filePath the filepath
   */
  public GetFileName(filePath: string): string {
    const result = filePath.split("/").pop();
    if (!result) return filePath;
    return result;
  }

  /**
   * extract fileName Without Extension from string
   * @param filePath the filepath
   */
  public GetFileNameWithoutExtension(filePath: string) {
    const fileName = this.GetFileName(filePath);
    return fileName.replace(/\.[a-zA-Z0-9]{1,4}$/, "");
  }

  /**
   * Get File Extension without dot
   * @param fileNameWithDot the filepath
   */
  public GetFileExtensionWithoutDot(fileNameWithDot: string) {
    if (fileNameWithDot.indexOf(".") === -1) return "";

    const extensionRegex = /[^.][a-zA-Z0-9]{1,4}$/;
    const fileNameMatchArray = extensionRegex.exec(fileNameWithDot);

    if (!fileNameMatchArray) return "";
    return fileNameMatchArray[0].toLowerCase();
  }
}
