import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { IUrl } from '../interfaces/IUrl';

export class URLPath {

  public FileNameBreadcrumb(filePath: string) {
    return filePath.split("/")[filePath.split("/").length - 1]
  }

  public StringToIUrl(locationHash: string): IUrl {
    let hash = this.RemovePrefixUrl(locationHash);
    let params = new URLSearchParams(hash).entries();

    var urlObject: IUrl = {}
    for (let key of Array.from(params)) {
      switch (key[0].toLowerCase()) {
        case 'colorClass'.toLowerCase():
          const colorClassText = key[1];
          urlObject.colorClass = this.stringToNumberArray(colorClassText)
          break;
        case 'collections'.toLowerCase():
          // default is true
          if (key[1] === "false") {
            urlObject.collections = false
          }
          else {
            urlObject.collections = true
          }
          break;
        case 'details'.toLowerCase():
          if (key[1] === "true") {
            urlObject.details = true
          }
          else {
            urlObject.details = false
          }
          break;
        case 'sidebar'.toLowerCase():
          if (key[1] === "true") {
            urlObject.sidebar = true
          }
          else {
            urlObject.sidebar = false
          }
          break;
        case 'f':
          urlObject.f = key[1];
          break;
        case 't': // used for search queries
          urlObject.t = key[1];
          break;
        case 'p': // used for search pagination
          var pagination = Number(key[1]);
          if (isNaN(pagination)) continue;
          urlObject.p = pagination;
          break;
        case 'select'.toLowerCase():
          // remove?
          // if (key[1] === 'null') {
          //   continue;
          // }
          urlObject.select = this.getStringArrayFromCommaSeparatedString(key[1]);
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
    var colorClassArray: Array<string> = [];
    if (colorClassText && (colorClassText.indexOf(",") === -1)) {
      colorClassArray = [colorClassText]
    }
    else if (colorClassText.indexOf(",") >= 1) {
      colorClassText.split(",").forEach(element => {
        colorClassArray.push(element);
      });
    }
    return colorClassArray;
  }

  public GetReturnUrl(locationHash: string): string {
    // ?ReturnUrl=%2F
    let hash = this.RemovePrefixUrl(locationHash);
    let search = new URLSearchParams(hash);
    let getReturnUrl = search.get("ReturnUrl");
    if (!getReturnUrl) return "/" + this.addPrefixUrl("f=/");
    return getReturnUrl;
  }

  /**
   * Convert a comma separated string to a Array of numbers
   * @param colorClassText 
   */
  private stringToNumberArray(colorClassText: string): number[] {
    var colorClassArray: Array<number> = [];
    if (colorClassText && !isNaN(Number(colorClassText))) {
      colorClassArray = [Number(colorClassText)]
    }
    else if (colorClassText.indexOf(",") >= 1) {
      colorClassText.split(",").forEach(element => {
        if (!isNaN(Number(element))) {
          colorClassArray.push(Number(element))
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
    var params = new URLSearchParams();
    for (let key of Object.entries(urlObject)) {
      params.set(key[0], key[1]);
    }
    var url = this.addPrefixUrl(params.toString());
    url = url.replace(/\+/ig, " ").replace(/%2F/ig, "/").replace(/%2C/ig, ",");
    return url;
  }

  public encodeURI(url: string): string {
    url = encodeURI(url);
    url = url.replace(/\+/ig, "%2B");
    return url;
  }

  /**
 * append=true&collections=true&tags=update
 * @param toUpdate 
 */
  public ObjectToSearchParams(toUpdate: Object): URLSearchParams {
    var bodyParams = new URLSearchParams();
    for (let key of Object.entries(toUpdate)) {
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
    let output = input.replace(/^#?(\/)?/ig, "");
    return output.replace(/\+/ig, "%2B");
  }

  private addPrefixUrl(input: string): string {
    return "?" + input;
  }

  public getChild(getFilePath: string): string {
    if (!getFilePath) return "";
    getFilePath = this.removeEndOnSlash(getFilePath);
    var result = getFilePath.split("/")[getFilePath.split("/").length - 1]
    return result;
  }

  public getParent(locationHash: string): string {
    let hash = this.RemovePrefixUrl(locationHash);
    let search = new URLSearchParams(hash);
    let getFilePath = search.get("f");

    if (!getFilePath) return "/";
    getFilePath = this.endOnSlash(getFilePath);

    var parentPath = "";
    var filePathArray = getFilePath.split("/");
    for (let index = 0; index < filePathArray.length; index++) {
      const element = filePathArray[index];
      if (index <= filePathArray.length - 3) {
        parentPath += element + "/";
      }
    }
    if (filePathArray.length <= 3) return "/";

    parentPath = this.startOnSlash(parentPath);
    parentPath = this.removeEndOnSlash(parentPath);
    return parentPath;
  }

  private removeEndOnSlash(input: string): string {
    if (!input.endsWith("/")) return input;
    var output = input.substring(0, input.length - 1);
    return output;
  }

  private startOnSlash(input: string): string {
    if (input.startsWith("/")) return input;
    return "/" + input;
  }

  private endOnSlash(input: string): string {
    if (input.endsWith("/")) return input;
    return input + "/";
  }

  public getFilePath(locationHash: string): string {
    let hash = this.RemovePrefixUrl(locationHash);
    let search = new URLSearchParams(hash);
    let getFilePath = search.get("f");
    if (!getFilePath) return "/";
    return getFilePath.replace(/\/$/, "");
  }

  /**
   * Keep colorClass in URL
   */
  public updateFilePath(historyLocationHash: string, toUpdateFilePath: string): string {
    var url = new URLPath().StringToIUrl(historyLocationHash);
    url.f = toUpdateFilePath;
    return "/" + new URLPath().IUrlToString(url);
  }

  /**
   * updateSelection
   */
  public updateSelection(historyLocationHash: string, toUpdateSelect: string[]): IUrl {
    var urlObject = new URLPath().StringToIUrl(historyLocationHash);
    urlObject.select = toUpdateSelect;
    return urlObject;
  }

  public toggleSelection(fileName: string, locationHash: string): IUrl {

    var urlObject = new URLPath().StringToIUrl(locationHash);
    if (!urlObject.select) {
      urlObject.select = [];
    }

    if (!urlObject.select || urlObject.select.indexOf(fileName) === -1) {
      urlObject.select.push(fileName)
    }
    else {
      var index = urlObject.select.indexOf(fileName);
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
    var selectResult = new URLPath().StringToIUrl(historyLocationSearch).select
    if (selectResult !== undefined) {
      selectList = selectResult;
    }
    return selectList;
  }

  public MergeSelectParent(select: string[] | undefined, parent: string | undefined): string[] {
    var subPaths: string[] = [];
    if (select === undefined || parent === undefined) return subPaths;

    select.forEach(item => {
      subPaths.push(parent + "/" + item)
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
    fileIndexItems.forEach(fileIndexItem => {
      var include = select.includes(fileIndexItem.fileName);
      if (!include) {
        select.push(fileIndexItem.fileName)
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
    var subPaths: string[] = [];

    fileIndexItems.forEach(item => {
      if (select.indexOf(item.fileName) >= 0) {
        subPaths.push(item.parentDirectory + "/" + item.fileName)
      }
    });
    return subPaths;
  }

  public ArrayToCommaSeperatedStringOneParent(select: string[], parent: string): string {
    var selectParams = "";
    for (let index = 0; index < select.length; index++) {
      const element = select[index];
      selectParams += parent + "/" + element;
      if (index !== select.length - 1) {
        selectParams += ";";
      }
    }
    return selectParams;
  }

}