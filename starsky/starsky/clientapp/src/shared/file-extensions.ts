
export class FileExtensions {

  public MatchExtension(from: string, to: string): (boolean | null) {
    var extensionRegex = /\.[0-9a-z]+$/;

    var fromExtMatchArray = from.match(extensionRegex);
    if (!fromExtMatchArray) return null;

    var toExtMatchArray = to.match(extensionRegex);
    if (!toExtMatchArray) return false;
    return toExtMatchArray[0] === fromExtMatchArray[0];
  }
}

