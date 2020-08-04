import { IUrl, newIUrl } from '../interfaces/IUrl';
import { URLPath } from './url-path';

export class UrlQuery {

  public prefix: string = "/starsky"

  public UrlHomePage(): string {
    return document.location.pathname.indexOf(this.prefix) === -1 ? "/" : `${this.prefix}/`;
  }

  public UrlHomeIndexPage(locationHash: string): string {
    return document.location.pathname.indexOf(this.prefix) === -1 ? `${new URLPath().StartOnSlash(locationHash)}` : `${this.prefix}${new URLPath().StartOnSlash(locationHash)}`;
  }

  /**
   * Parse the return url based on the query
   * Does NOT add prefix
   * @param locationHash location .search hash
   */
  public GetReturnUrl(locationHash: string): string {
    // ?ReturnUrl=%2F
    let hash = new URLPath().RemovePrefixUrl(locationHash);
    let search = new URLSearchParams(hash);
    let getReturnUrl = search.get("ReturnUrl");

    // add only prefix for default situation
    if (!getReturnUrl) return `/${new URLPath().AddPrefixUrl("f=/")}`;
    return getReturnUrl;
  }

  /**
   * Get the search page
   * @param t query
   */
  public UrlSearchPage(t: string): string {
    return document.location.pathname.indexOf(this.prefix) === -1 ? `/search?t=${t}` : `${this.prefix}/search?t=${t}`;
  }

  /**
  * Search path based on Location Hash
  */
  public HashSearchPage(historyLocationHash: string): string {
    var url = new URLPath().StringToIUrl(historyLocationHash);
    return document.location.pathname.indexOf(this.prefix) === -1 ? `/search${new URLPath().IUrlToString(url)}` : `${this.prefix}/search${new URLPath().IUrlToString(url)}`;
  }

  public UrlTrashPage(): string {
    return document.location.pathname.indexOf(this.prefix) === -1 ? `/trash` : `${this.prefix}/trash`;
  }

  public UrlImportPage(): string {
    return document.location.pathname.indexOf(this.prefix) === -1 ? `/import` : `${this.prefix}/import`;
  }

  public UrlPreferencesPage(): string {
    return document.location.pathname.indexOf(this.prefix) === -1 ? `/preferences` : `${this.prefix}/preferences`;
  }

  public UrlLoginPage(): string {
    return document.location.pathname.indexOf(this.prefix) === -1 ? `/account/login` : `${this.prefix}/account/login`;
  }

  public UrlLogoutPage(returnUrl: string): string {
    return document.location.pathname.indexOf(this.prefix) === -1 ? `/account/logout?ReturnUrl=${returnUrl}` : `${this.prefix}/account/logout?ReturnUrl=${returnUrl}`;
  }


  private urlReplacePath(input: string): string {
    let output = input.replace("#", "");
    return output.replace(/\+/ig, "%2B");
  }

  public UrlAccountRegister(): string {
    return `${this.prefix}/account/register?json=true`;
  }

  public UrlSearchRelativeApi = (f: string, t: string | undefined, pageNumber = 0): string => {
    return `${this.prefix}/api/search/relativeObjects?f=` + new URLPath().encodeURI(f) + "&t=" +
      t +
      "&p=" + pageNumber;
  }

  public UrlQuerySearchApi = (query: string, pageNumber = 0): string => {
    return this.prefix + "/api/search?json=true&t=" + query + "&p=" + pageNumber;
  }

  public UrlSearchSuggestApi(query: string): string {
    return this.prefix + "/api/suggest/?t=" + query;
  }

  public UrlSearchRemoveCacheApi(): string {
    return this.prefix + "/api/search/removeCache";
  }

  public UrlSearchTrashApi = (pageNumber = 0): string => {
    return this.prefix + "/api/search/trash?p=" + pageNumber;
  }

  public UrlAccountStatus = (): string => {
    return this.prefix + "/account/status";
  }

  public UrlAccountRegisterStatus = (): string => {
    return this.prefix + "/account/register/status";
  }

  public UrlAccountChangeSecret = (): string => {
    return this.prefix + "/api/account/change-secret";
  }

  public UrlAccountPermissions = (): string => {
    return this.prefix + "/api/account/permissions";
  }

  /**
   * Keep colorClass in URL
   */
  public updateFilePathHash(historyLocationHash: string, toUpdateFilePath: string, clearTSearchQuery?: boolean): string {
    var url = new URLPath().StringToIUrl(historyLocationHash);
    url.f = toUpdateFilePath;
    // when browsing to a parent folder from a detailview item
    if (clearTSearchQuery) {
      delete url.t;
      delete url.p;
    }
    return document.location.pathname.indexOf(this.prefix) === -1 ? `/${new URLPath().IUrlToString(url)}` : `${this.prefix}/${new URLPath().IUrlToString(url)}`;
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
    return this.prefix + "/api/index" + new URLPath().IUrlToString(urlObject)
  }

  /**
   * Get Direct api/index with IUrl
   */
  public UrlIndexServerApiPath = (path: string): string => {
    return this.prefix + "/api/index?f=" + path
  }

  /**
   * GET: Gets the realtime API
   * @param subPath subpath style
   */
  public UrlQueryInfoApi(subPath: string): string {
    if (!subPath) return "";
    var url = this.urlReplacePath(subPath);
    return this.prefix + "/api/info?f=" + url + "&json=true";
  }

  /**
   * POST to this to update meta information
   */
  public UrlUpdateApi = (): string => {
    return this.prefix + "/api/update";
  }

  /**
   * POST to this to search and replace meta information like: tags, descriptions and titles
   */
  public UrlReplaceApi = (): string => {
    return this.prefix + "/api/replace";
  }

  /**
  * DELETE to endpoint to remove file from database and disk
  */
  public UrlDeleteApi = (): string => {
    return this.prefix + "/api/delete";
  }

  public UrlThumbnailImage = (fileHash: string, issingleitem: boolean): string => {
    return this.prefix + "/api/thumbnail/" + fileHash + ".jpg?issingleitem=" + issingleitem.toString();
  }

  public UrlThumbnailJsonApi = (fileHash: string): string => {
    return this.prefix + "/api/thumbnail/" + fileHash + "?json=true";
  }

  // http://localhost:5000/api/downloadPhoto?f=%2F__starsky%2F0001-readonly%2F4.jpg&isThumbnail=True
  public UrlDownloadPhotoApi = (f: string, isThumbnail: boolean = true): string => {
    return this.prefix + "/api/downloadPhoto?f=" + f + "&isThumbnail=" + isThumbnail;
  }

  public UrlApiAppSettings = (): string => {
    return this.prefix + "/api/env/"
  }

  /**
   * url create a zip
   */
  public UrlExportPostZipApi = (): string => {
    return this.prefix + "/export/createZip/"
  }

  /**
   * export/zip/SR497519527.zip?json=true
   */
  public UrlExportZipApi = (createZipId: string, json: boolean = true): string => {
    return this.prefix + "/export/zip/" + createZipId + ".zip?json=" + json;
  }

  public UrlSync(parentFolder: string): string {
    return this.prefix + "/sync?f=" + new URLPath().encodeURI(parentFolder);
  }

  /**
   * Rename the file on disk and in the database
   */
  public UrlSyncRename(): string {
    return this.prefix + "/sync/rename";
  }

  /**
   * Create an directory on disk and database
   */
  public UrlSyncMkdir(): string {
    return this.prefix + "/sync/mkdir";
  }

  public UrlImportApi(): string {
    return this.prefix + "/api/import";
  }

  public UrlUploadApi(): string {
    return this.prefix + "/api/upload";
  }

  public UrlAllowedTypesThumb(filename: string): string {
    return this.prefix + "/api/allowed-types/thumb?f=" + filename;
  }

  public UrlHealthDetails(): string {
    return `${this.prefix}/api/health/details`
  }

  public UrlRemoveCache(parentFolder: string): string {
    return this.prefix + "/api/RemoveCache?json=true&f=" + parentFolder
  }

  public UrlGeoSync(): string {
    return this.prefix + "/api/geo/sync";
  }

  public UrlGeoStatus(arg0: string): string {
    return this.prefix + "/api/geo/status/?f=" + arg0;
  }

  public UrlThumbnailGeneration(): string {
    return this.prefix + "/api/thumbnail-generation";
  }

}
