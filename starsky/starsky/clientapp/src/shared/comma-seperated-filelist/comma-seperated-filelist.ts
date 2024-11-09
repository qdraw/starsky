export class CommaSeperatedFileList {
  public CommaSpaceLastDot(inputs: string[], messageNoExtensionItem?: string): string {
    let output = "";
    const uniqueExtensionsArray = this.GetUniqueExtensions(inputs, messageNoExtensionItem);

    for (let index = 0; index < uniqueExtensionsArray.length; index++) {
      output += uniqueExtensionsArray[index];
      if (index !== uniqueExtensionsArray.length - 1) {
        output += ", ";
      }
    }
    return output;
  }

  private GetUniqueExtensions(inputs: string[], messageNoExtensionItem?: string): Array<string> {
    const uniqueExtensions = new Set<string>();
    for (const element of inputs) {
      const fileExtension = element.split(".")[element.split(".").length - 1];
      if (element !== fileExtension) {
        uniqueExtensions.add(fileExtension);
      } else {
        uniqueExtensions.add(messageNoExtensionItem || "without extension");
      }
    }
    return Array.from(uniqueExtensions).sort((a, b) =>
      a.localeCompare(b, "en", { sensitivity: "base" })
    );
  }
}
