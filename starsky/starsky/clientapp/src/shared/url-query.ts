import { newIUrl } from '../interfaces/IUrl';
import { URLPath } from './url-path';


export class UrlQuery {

  private urlReplacePath(input: string): string {
    let output = input.replace("#", "");
    return output.replace(/\+/ig, "%2B");
  }

  public UrlLogin() {
    return "/account/login?json=true";
  }

  public UrlQuerySearchApi = (query: string, pageNumber = 0) => {
    return "/api/search?json=true&t=" + query + "&p=" + pageNumber;
  }

  public UrlSearchTrashApi = (pageNumber = 0) => {
    return "/api/search/trash?p=" + pageNumber;
  }

  public UrlAccountStatus = () => {
    return "/account/status";
  }

  /**
   * Used with localisation hash
   */
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
    if (urlObject.f !== undefined) {
      return this.UrlIndexServerApi(urlObject.f);
    }
    return this.UrlIndexServerApi("/");
  }

  /**
   * Get Direct api/index
   */
  public UrlIndexServerApi = (subPath: string) => {
    return "/api/index?f=" + new URLPath().encodeURI(subPath);
  }

  public UrlQueryInfoApi(subPath: string): string {
    if (!subPath) return "";
    var url = this.urlReplacePath(subPath);
    return "/api/info?f=" + url + "&json=true";
  }

  public UrlUpdateApi = () => {
    return "/api/update";
  }

  public UrlReplaceApi = () => {
    return "/api/replace";
  }

  public UrlThumbnailImage = (fileHash: string) => {
    return "/api/thumbnail/" + fileHash + ".jpg?issingleitem=true";
  }

  public UrlThumbnailJsonApi = (fileHash: string) => {
    return "/api/thumbnail/" + fileHash + "?json=true";
  }

  // http://localhost:5000/api/downloadPhoto?f=%2F__starsky%2F0001-readonly%2F4.jpg&isThumbnail=True
  public UrlDownloadPhotoApi = (f: string, isThumbnail: boolean = true) => {
    return "/api/downloadPhoto?f=" + f + "&isThumbnail=" + isThumbnail;
  }

  /**
   * url create a zip
   */
  public UrlExportPostZipApi = () => {
    return "/export/createZip/"
  }

  /**
   * export/zip/SR497519527.zip?json=true
   */
  public UrlExportZipApi = (createZipId: string, json: boolean = true) => {
    return "/export/zip/" + createZipId + ".zip?json=" + json;
  }

  /**
   * Rename the file on disk and in the database
   */
  public UrlSyncRename(): string {
    return "/sync/rename";
  }

}
