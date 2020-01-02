
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

}

