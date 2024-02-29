import { SortType } from "../../interfaces/IArchive";
import { IFileIndexItem } from "../../interfaces/IFileIndexItem";
import { IUrl } from "../../interfaces/IUrl";

export class URLPath {
  public FileNameBreadcrumb(filePath: string) {
    if (!filePath) return "/";
    return filePath.split("/")[filePath.split("/").length - 1];
  }

  private parsePagination(key: [string, string], urlObject: IUrl) {
    const pagination = Number(key[1]);
    if (isNaN(pagination)) return;
    urlObject.p = pagination;
  }

  public StringToIUrl(locationHash: string): IUrl {
    const hash = this.RemovePrefixUrl(locationHash);
    const params = new URLSearchParams(hash).entries();

    const urlObject: IUrl = {};
    for (const key of Array.from(params)) {
      switch (key[0].toLowerCase()) {
        case "colorClass".toLowerCase():
          urlObject.colorClass = this.stringToNumberArray(key[1]);
          break;
        case "collections".toLowerCase():
          // default is true
          if (key[1] === "false") {
            urlObject.collections = false;
          } else {
            urlObject.collections = true;
          }
          break;
        case "details".toLowerCase():
          urlObject.details = key[1] === "true";
          break;
        case "sidebar".toLowerCase():
          urlObject.sidebar = key[1] === "true";
          break;
        case "f":
          urlObject.f = key[1];
          break;
        case "t": // used for search queries
          urlObject.t = key[1];
          break;
        case "p": // used for search pagination
          this.parsePagination(key, urlObject);
          break;
        case "select".toLowerCase():
          urlObject.select = this.getStringArrayFromCommaSeparatedString(key[1]);
          break;
        case "sort".toLowerCase():
          urlObject.sort = SortType[key[1] as keyof typeof SortType];
          break;
        case "list".toLowerCase():
          urlObject.list = key[1] === "true";
          break;
        default:
          break;
      }
    }
    return urlObject;
  }

  /**
   * Convert a comma separated string to a Array of strings
   * @param colorClassText
   */
  private getStringArrayFromCommaSeparatedString(colorClassText: string): string[] {
    let colorClassArray: Array<string> = [];
    if (colorClassText && colorClassText.indexOf(",") === -1) {
      colorClassArray = [colorClassText];
    } else if (colorClassText.indexOf(",") >= 1) {
      colorClassText.split(",").forEach((element) => {
        colorClassArray.push(element);
      });
    }
    return colorClassArray;
  }

  /**
   * Convert a comma separated string to a Array of numbers
   * @param colorClassText
   */
  private stringToNumberArray(colorClassText: string): number[] {
    let colorClassArray: Array<number> = [];
    if (colorClassText && !isNaN(Number(colorClassText))) {
      colorClassArray = [Number(colorClassText)];
    } else if (colorClassText.indexOf(",") >= 1) {
      colorClassText.split(",").forEach((element) => {
        if (!isNaN(Number(element))) {
          colorClassArray.push(Number(element));
        }
      });
    }
    return colorClassArray;
  }

  /**
   * Write it down to a string
   * @param urlObject Casted Object that holds the url state
   */
  public IUrlToString(urlObject: IUrl): string {
    const params = new URLSearchParams();
    for (const key of Object.entries(urlObject)) {
      params.set(key[0], key[1]);
    }
    let url = this.AddPrefixUrl(params.toString());
    url = url.replace(/\+/gi, " ").replace(/%2F/gi, "/").replace(/%2C/gi, ",");
    return url;
  }

  public encodeURI(url: string): string {
    url = encodeURI(url);
    url = url.replace(/\+/gi, "%2B");
    return url;
  }

  /**
   * append=true&collections=true&tags=update
   * @param toUpdate
   */
  // eslint-disable-next-line @typescript-eslint/ban-types
  public ObjectToSearchParams(toUpdate: Object): URLSearchParams {
    const bodyParams = new URLSearchParams();
    for (const key of Object.entries(toUpdate)) {
      if (key[1] && key[1].length >= 1) {
        bodyParams.set(key[0], key[1]);
      }
      if (key[1] === true || key[1] === false) {
        bodyParams.set(key[0], key[1]);
      }
    }
    return bodyParams;
  }

  public RemovePrefixUrl(input: string): string {
    if (!input) return "";
    const output = input.replace(/^#?(\/)?/gi, "");
    return output.replace(/\+/gi, "%2B");
  }

  /**
   * Add query string ? before url
   * @param input url
   */
  public AddPrefixUrl(input: string): string {
    return "?" + input;
  }

  public getChild(getFilePath: string): string {
    if (!getFilePath) return "";
    getFilePath = this.removeEndOnSlash(getFilePath);
    return getFilePath.split("/")[getFilePath.split("/").length - 1];
  }

  public getParent(locationHash: string): string {
    const hash = this.RemovePrefixUrl(locationHash);
    const search = new URLSearchParams(hash);
    let getFilePath = search.get("f");

    if (!getFilePath) return "/";
    getFilePath = this.endOnSlash(getFilePath);

    let parentPath = "";
    const filePathArray = getFilePath.split("/");
    for (let index = 0; index < filePathArray.length; index++) {
      const element = filePathArray[index];
      if (index <= filePathArray.length - 3) {
        parentPath += element + "/";
      }
    }
    if (filePathArray.length <= 3) return "/";

    parentPath = this.StartOnSlash(parentPath);
    parentPath = this.removeEndOnSlash(parentPath);
    return parentPath;
  }

  private removeEndOnSlash(input: string): string {
    if (!input.endsWith("/")) return input;
    return input.substring(0, input.length - 1);
  }

  public StartOnSlash(input: string): string {
    if (!input) throw new Error("should pass any input");
    if (input.startsWith("/")) return input;
    return "/" + input;
  }

  private endOnSlash(input: string): string {
    if (input.endsWith("/")) return input;
    return input + "/";
  }

  public getFilePath(locationHash: string): string {
    const hash = this.RemovePrefixUrl(locationHash);
    const search = new URLSearchParams(hash);
    const getFilePath = search.get("f");
    if (!getFilePath) return "/";
    return getFilePath.replace(/\/$/, "");
  }

  /**
   * updateSelection
   */
  public updateSelection(historyLocationHash: string, toUpdateSelect: string[]): IUrl {
    const urlObject = new URLPath().StringToIUrl(historyLocationHash);
    urlObject.select = toUpdateSelect;
    return urlObject;
  }

  public toggleSelection(fileName: string, locationHash: string): IUrl {
    const urlObject = new URLPath().StringToIUrl(locationHash);
    if (!urlObject.select) {
      urlObject.select = [];
    }

    if (!urlObject.select || urlObject.select.indexOf(fileName) === -1) {
      urlObject.select.push(fileName);
    } else {
      const index = urlObject.select.indexOf(fileName);
      if (index !== -1) urlObject.select.splice(index, 1);
    }
    return urlObject;
  }

  /**
   * Get an non-null list
   * @param historyLocationSearch
   */
  public getSelect(historyLocationSearch: string) {
    let selectList = new Array<string>();
    const selectResult = new URLPath().StringToIUrl(historyLocationSearch).select;
    if (selectResult !== undefined) {
      selectList = selectResult;
    }
    return selectList;
  }

  public MergeSelectParent(select: string[] | undefined, parent: string | undefined): string[] {
    const subPaths: string[] = [];
    if (select === undefined || parent === undefined) return subPaths;

    select.forEach((item) => {
      if (parent === "/") {
        subPaths.push("/" + item);
      } else {
        subPaths.push(parent + "/" + item);
      }
    });
    return subPaths;
  }

  /**
   * To give back a fileName list of all items
   * Merge without parent path
   * @param select the current selection
   * @param fileIndexItems the current folder
   */
  public GetAllSelection(select: string[], fileIndexItems: IFileIndexItem[]): string[] {
    fileIndexItems.forEach((fileIndexItem) => {
      const include = select.includes(fileIndexItem.fileName);
      if (!include) {
        select.push(fileIndexItem.fileName);
      }
    });
    return select;
  }

  /**
   * Merge with parent path
   * @param select List of items that are already selected
   * @param fileIndexItems
   */
  public MergeSelectFileIndexItem(select: string[], fileIndexItems: IFileIndexItem[]): string[] {
    const subPaths: string[] = [];

    fileIndexItems.forEach((item) => {
      if (item.fileName && select.indexOf(item.fileName) >= 0) {
        if (item.parentDirectory === "/") item.parentDirectory = ""; // no double slash in front of path
        subPaths.push(item.parentDirectory + new URLPath().StartOnSlash(item.fileName));
      }
    });
    return subPaths;
  }

  /**
   * Combine select to dot comma Separated
   * @param select Array with path
   */
  public ArrayToCommaSeparatedString(select: string[]): string {
    let selectString = "";
    for (let index = 0; index < select.length; index++) {
      const element = select[index];
      if (index === 0) {
        selectString = element;
        continue;
      }
      selectString += ";" + element;
    }
    return selectString;
  }

  public ArrayToCommaSeparatedStringOneParent(select: string[], parent: string): string {
    let selectParams = "";
    for (let index = 0; index < select.length; index++) {
      const element = select[index];

      // no double slash in front of path
      const slash = !parent && element.startsWith("/") ? "" : "/";
      selectParams += parent + slash + element;

      if (index !== select.length - 1) {
        selectParams += ";";
      }
    }
    return selectParams;
  }
}
