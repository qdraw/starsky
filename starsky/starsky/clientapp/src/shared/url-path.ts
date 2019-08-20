import { IUrl } from '../interfaces/IUrl';

export class URLPath {

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
        case 'details'.toLowerCase():
          if (key[1] === "true") {
            urlObject.details = true
          }
          break;
        case 'f':
          urlObject.f = key[1];
          break;
        case 'sidebar'.toLowerCase():
          if (key[1] === 'null') continue;
          urlObject.sidebar = this.getStringArrayFromCommaSeparatedString(key[1]);
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

  public IUrlToString(urlObject: IUrl): string {
    var params = new URLSearchParams();
    for (let key of Object.entries(urlObject)) {
      params.set(key[0], key[1]);
    }
    var url = this.addPrefixUrl(params.toString());
    return url.replace(/\+/ig, " ").replace(/%2F/ig, "/").replace(/%2C/ig, ",");
  }


  public RemovePrefixUrl(input: string): string {
    let output = input.replace(/^#(\/)?/ig, "");
    return output.replace(/\+/ig, "%2B");
  }

  private addPrefixUrl(input: string): string {
    return "#?" + input;
  }

  public getParent(locationHash: string): string {
    let hash = this.RemovePrefixUrl(locationHash);
    let search = new URLSearchParams(hash);
    let getFilePath = search.get("f");

    if (!getFilePath) return "/";
    var array = getFilePath.split('/');
    return getFilePath.replace(array[array.length - 1], '');
  }

  public getFilePath(locationHash: string): string {
    let hash = this.RemovePrefixUrl(locationHash);
    let search = new URLSearchParams(hash);
    let getFilePath = search.get("f");
    if (!getFilePath) return "/";
    return getFilePath;
  }


  /**
   * Keep colorClass in URL
   */
  public updateFilePath(historyLocationHash: string, toUpdateFilePath: string): string {
    var url = new URLPath().StringToIUrl(historyLocationHash);
    url.f = toUpdateFilePath;
    return new URLPath().IUrlToString(url);
  }

}