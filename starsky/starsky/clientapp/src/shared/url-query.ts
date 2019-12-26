import { IUrl, newIUrl } from '../interfaces/IUrl';
import { URLPath } from './url-path';


export class UrlQuery {

  private urlReplacePath(input: string): string {
    let output = input.replace("#", "");
    return output.replace(/\+/ig, "%2B");
  }

  public UrlLogin(): string {
    return "/account/login?json=true";
  }

  public UrlSearchRelativeApi = (f: string, t: string | undefined, pageNumber = 0): string => {
    return "/api/search/relativeObjects?f=" + f + "&t=" +
      t +
      "&p=" + pageNumber;
  }

  public UrlQuerySearchApi = (query: string, pageNumber = 0): string => {
    return "/api/search?json=true&t=" + query + "&p=" + pageNumber;
  }

  public UrlSearchTrashApi = (pageNumber = 0): string => {
    return "/api/search/trash?p=" + pageNumber;
  }

  public UrlAccountStatus = (): string => {
    return "/account/status";
  }

  /**
   * Used with localisation hash
   */
  public UrlQueryServerApi = (historyLocationHash: string): string => {
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
    return this.UrlIndexServerApi(urlObject);
  }

  /**
   * Get Direct api/index with IUrl
   */
  public UrlIndexServerApi = (urlObject: IUrl): string => {
    return "/api/index" + new URLPath().IUrlToString(urlObject)
  }

  /**
   * GET: Gets the realtime API
   * @param subPath subpath style
   */
  public UrlQueryInfoApi(subPath: string): string {
    if (!subPath) return "";
    var url = this.urlReplacePath(subPath);
    return "/api/info?f=" + url + "&json=true";
  }

  /**
   * POST to this to update meta information
   */
  public UrlUpdateApi = (): string => {
    return "/api/update";
  }

  /**
   * POST to this to search and replace meta information like: tags, descriptions and titles
   */
  public UrlReplaceApi = (): string => {
    return "/api/replace";
  }

  public UrlThumbnailImage = (fileHash: string): string => {
    return "/api/thumbnail/" + fileHash + ".jpg?issingleitem=true";
  }

  public UrlThumbnailJsonApi = (fileHash: string): string => {
    return "/api/thumbnail/" + fileHash + "?json=true";
  }

  // http://localhost:5000/api/downloadPhoto?f=%2F__starsky%2F0001-readonly%2F4.jpg&isThumbnail=True
  public UrlDownloadPhotoApi = (f: string, isThumbnail: boolean = true): string => {
    return "/api/downloadPhoto?f=" + f + "&isThumbnail=" + isThumbnail;
  }

  /**
   * url create a zip
   */
  public UrlExportPostZipApi = (): string => {
    return "/export/createZip/"
  }

  /**
   * export/zip/SR497519527.zip?json=true
   */
  public UrlExportZipApi = (createZipId: string, json: boolean = true): string => {
    return "/export/zip/" + createZipId + ".zip?json=" + json;
  }

  /**
   * Rename the file on disk and in the database
   */
  public UrlSyncRename(): string {
    return "/sync/rename";
  }

}
