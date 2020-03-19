
export class FileExtensions {

  /**
   * Match two filenames, make sure those are having the same extensions
   * @param from input.jpg
   * @param to changeto.jpg
   */
  public MatchExtension(from: string, to: string): (boolean | null) {
    var extensionRegex = /\.[0-9a-z]+$/;

    var fromExtMatchArray = from.match(extensionRegex);
    if (!fromExtMatchArray) return null;

    var toExtMatchArray = to.match(extensionRegex);
    if (!toExtMatchArray) return false;
    return toExtMatchArray[0] === fromExtMatchArray[0];
  }

  /**
   * Checks if the filename is valid
   * @param filename 
   */
  public IsValidFileName(filename: string): (boolean) {
    var extensionRegex = /^[a-zA-Z0-9_](?:[a-zA-Z0-9 ._-]*[a-zA-Z0-9])?\.[a-zA-Z0-9_-]+$/
    var fromExtMatchArray = filename.match(extensionRegex);
    return !!fromExtMatchArray;
  }

  /**
   * Checks if the directory name is valid
   * @param directoryName only the name, not the full path
   */
  public IsValidDirectoryName(directoryName: string): (boolean) {
    var extensionRegex = /^[$a-zA-Z0-9_\s-]{2,}$/
    var fromDirMatchArray = directoryName.match(extensionRegex);
    return !!fromDirMatchArray;
  }

  /**
   * Get the parent path from your string 
   * @param filePath filepath
   * @see https://stackoverflow.com/a/1130024
   */
  public GetParentPath(filePath: string) {
    if (!filePath) return "/"
    const parentRegex = /.+(?=\/[^/]+$)/;

    // remove slash from end
    if (filePath.length >= 2 && filePath[filePath.length - 1] === "/") {
      filePath = filePath.substr(0, filePath.length - 1)
    }

    const parentMatchArray = filePath.match(parentRegex);
    if (!parentMatchArray) return "/";
    return parentMatchArray[0]
  }

  /**
   * extract fileName from string
   * @param filePath the filepath
   */
  public GetFileName(filePath: string) {
    // [^\/]+(?=\.[\w]+\.$)|[^\/]+$
    var filenameRegex = /[^/]+(?=\.[\w]+\.$)|[^/]+$/
    var fileNameMatchArray = filePath.match(filenameRegex);
    if (!fileNameMatchArray) return "/";
    return fileNameMatchArray[0]
  }

  /**
   * extract fileName Without Extension from string
   * @param filePath the filepath
   */
  public GetFileNameWithoutExtension(filePath: string) {
    var fileName = this.GetFileName(filePath);
    return fileName.replace(new RegExp("\.[a-zA-Z0-9]{1,4}$"), "")
  }

  /**
 * Get File Extension without dot
 * @param filename the filepath
 */
  public GetFileExtensionWithoutDot(fileNameWithDot: string) {
    if (fileNameWithDot.indexOf(".") === -1) return "";
    var fileNameMatchArray = fileNameWithDot.match(new RegExp("[^.][a-zA-Z0-9]{1,4}$"));
    if (!fileNameMatchArray) return "";
    return fileNameMatchArray[0].toLowerCase();
  }

}

