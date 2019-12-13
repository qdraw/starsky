
export class FileExtensions {

  public MatchExtension(from: string, to: string): (boolean | null) {
    var extensionRegex = /^([^\\]*)\.(\w+)$/;
    if (!from.match(extensionRegex)) return null;
    if (!to.match(extensionRegex)) return false;

    return null;
  }
}

