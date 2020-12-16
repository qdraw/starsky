export class FileExtensions {
  GetParentPath(filePath: string) {
    if (!filePath) return "/";
    const parentRegex = /.+(?=\/[^/]+$)/;

    // remove slash from end
    if (filePath.length >= 2 && filePath[filePath.length - 1] === "/") {
      filePath = filePath.substr(0, filePath.length - 1);
    }

    const parentMatchArray = filePath.match(parentRegex);
    if (!parentMatchArray) return "/";
    return parentMatchArray[0];
  }

  GetFileName(filePath: string) {
    // [^\/]+(?=\.[\w]+\.$)|[^\/]+$
    var filenameRegex = /[^/]+(?=\.[\w]+\.$)|[^/]+$/;
    var fileNameMatchArray = filePath.match(filenameRegex);
    if (!fileNameMatchArray) return "/";
    return fileNameMatchArray[0];
  }
}
