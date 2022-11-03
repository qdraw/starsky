/* eslint-disable class-methods-use-this */
export class FileExtensions {
  GetParentPath(filePath: string) {
    if (!filePath) return "/";
    const parentRegex = /.+(?=\/[^/]+$)/;

    // remove slash from end
    if (filePath.length >= 2 && filePath[filePath.length - 1] === "/") {
      // eslint-disable-next-line no-param-reassign
      filePath = filePath.substr(0, filePath.length - 1);
    }

    const parentMatchArray = filePath.match(parentRegex);
    if (!parentMatchArray) return "/";
    return parentMatchArray[0];
  }

  GetFileName(filePath: string) {
    // [^\/]+(?=\.[\w]+\.$)|[^\/]+$
    const filenameRegex = /[^/]+(?=\.[\w]+\.$)|[^/]+$/;
    const fileNameMatchArray = filePath.match(filenameRegex);
    if (!fileNameMatchArray) return "/";
    return fileNameMatchArray[0].replace("\n", "");
  }
}

export default FileExtensions;
