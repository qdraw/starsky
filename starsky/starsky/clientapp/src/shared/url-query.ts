import { newIUrl } from '../interfaces/IUrl';
import { URLPath } from './url-path';


export class UrlQuery {

  private urlReplacePath(input: string): string {
    let output = input.replace("#", "");
    return output.replace(/\+/ig, "%2B");
  }

  public UrlQuerySearchApi = (query: string, pageNumber = 0) => {
    return "/api/search?json=true&t=" + query + "&p=" + pageNumber;
  }

  public UrlSearchTrashApi = (pageNumber = 0) => {
    return "/api/search/trash?p=" + pageNumber;
  }

  public UrlQueryServerApi = (historyLocationHash: string) => {
    var requested = new URLPath().StringToIUrl(historyLocationHash);

    var urlObject = newIUrl();
    if (requested.f) {
      urlObject.f = requested.f;
    }
    if (requested.colorClass) {
      urlObject.colorClass = requested.colorClass;
    }
    if (requested.collections === false) {
      urlObject.collections = requested.collections;
    }
    // Not needed in API, but the context is used in detailview (without this the results in issues in the sidemenu)
    if (requested.details) {
      urlObject.details = requested.details;
    }
    var url = new URLPath().RemovePrefixUrl(new URLPath().IUrlToString(urlObject));
    return "/api/index" + url;
  }

  public UrlQueryInfoApi(subPath: string): string {
    if (!subPath) return "";
    var url = this.urlReplacePath(subPath);
    if (url === "") {
      url = "/";
    }
    return "/api/info?f=" + url + "&json=true";
  }

  public UrlQueryUpdateApi = () => {
    return "/api/update";
  }

  public UrlQueryThumbnailApi = (fileHash: string) => {
    return "/api/thumbnail/" + fileHash + "?json=true";
  }

  // http://localhost:5000/api/downloadPhoto?f=%2F__starsky%2F0001-readonly%2F4.jpg&isThumbnail=True
  public UrlDownloadPhotoApi = (f: string, isThumbnail: boolean = true) => {
    return "/api/downloadPhoto?f=" + f + "&isThumbnail=" + isThumbnail;
  }

  public UrlExportPostZipApi = () => {
    return "/export/createZip/"
  }

  // export/zip/SR497519527.zip?json=true
  public UrlExportZipApi = (createZipId: string, json: boolean = true) => {
    return "/export/zip/" + createZipId + ".zip?json=" + json;
  }

}
