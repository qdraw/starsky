import { IUrl, newIUrl } from '../interfaces/IUrl';
import { URLPath } from './url-path';


export class UrlQuery {

  private urlReplacePath(input: string): string {
    let output = input.replace("#", "");
    return output.replace(/\+/ig, "%2B");
  }

  public UrlLogin(): string {
    return "/account/login";
  }

  public UrlAccountRegister(): string {
    return "/account/register?json=true";
  }

  public UrlSearchRelativeApi = (f: string, t: string | undefined, pageNumber = 0): string => {
    return "/api/search/relativeObjects?f=" + new URLPath().encodeURI(f) + "&t=" +
      t +
      "&p=" + pageNumber;
  }

  public UrlQuerySearchApi = (query: string, pageNumber = 0): string => {
    return "/api/search?json=true&t=" + query + "&p=" + pageNumber;
  }

  public UrlSearch(query: string): string {
    return "/search?t=" + query
  }

  public UrlSearchSuggestApi(query: string): string {
    return "/api/suggest/?t=" + query;
  }

  public UrlSearchRemoveCacheApi(): string {
    return "/api/search/removeCache";
  }

  public UrlSearchTrashApi = (pageNumber = 0): string => {
    return "/api/search/trash?p=" + pageNumber;
  }

  public UrlAccountStatus = (): string => {
    return "/account/status";
  }

  public UrlAccountRegisterStatus = (): string => {
    return "/account/register/status";
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

  /**
  * DELETE to endpoint to remove file from database and disk
  */
  public UrlDeleteApi = (): string => {
    return "/api/delete";
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

  /**
   * Create an directory on disk and database
   */
  public UrlSyncMkdir(): string {
    return "/sync/mkdir";
  }

  public UrlImportApi(): string {
    return "/import";
  }

  public UrlUploadApi(): string {
    return "/api/upload";
  }

  public UrlAllowedTypesThumb(filename: string): string {
    return "/api/allowed-types/thumb?f=" + filename;
  }

}
