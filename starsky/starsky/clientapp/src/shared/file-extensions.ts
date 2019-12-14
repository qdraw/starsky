
export class FileExtensions {

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
  public ValidFileName(filename: string): (boolean) {
    var extensionRegex = /^[a-zA-Z0-9](?:[a-zA-Z0-9 ._-]*[a-zA-Z0-9])?\.[a-zA-Z0-9_-]+$/
    var fromExtMatchArray = filename.match(extensionRegex);
    return !!fromExtMatchArray;
  }

}

