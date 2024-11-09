export class CommaSeperatedFileList {
  public CommaSpaceLastDot(inputs: string[], messageNoExtensionItem?: string): string {
    let output = "";
    for (let index = 0; index < inputs.length; index++) {
      const element = inputs[index];
      const fileExtension = element.split(".")[element.split(".").length - 1];
      if (element !== fileExtension) {
        output += fileExtension;
      } else {
        output += messageNoExtensionItem ? messageNoExtensionItem : "without extension";
      }

      if (index !== inputs.length - 1) {
        output += ", ";
      }
    }
    return output;
  }
}
