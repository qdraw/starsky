import { IUrl, newIUrl } from "../interfaces/IUrl";
import isDev from "./is-dev";
import { URLPath } from "./url-path";

export class UrlQuery {
  public prefix: string = "/starsky";

  public UrlHomePage(): string {
    return document.location.pathname.indexOf(this.prefix) === -1
      ? "/"
      : `${this.prefix}/`;
  }

  public UrlHomeIndexPage(locationHash: string): string {
    return document.location.pathname.indexOf(this.prefix) === -1
      ? `${new URLPath().StartOnSlash(locationHash)}`
      : `${this.prefix}${new URLPath().StartOnSlash(locationHash)}`;
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
    return document.location.pathname.indexOf(this.prefix) === -1
      ? `/search?t=${t}`
      : `${this.prefix}/search?t=${t}`;
  }

  /**
   * Search path based on Location Hash
   */
  public HashSearchPage(historyLocationHash: string): string {
    const url = new URLPath().StringToIUrl(historyLocationHash);
    return document.location.pathname.indexOf(this.prefix) === -1
      ? `/search${new URLPath().IUrlToString(url)}`
      : `${this.prefix}/search${new URLPath().IUrlToString(url)}`;
  }

  public UrlTrashPage(): string {
    return document.location.pathname.indexOf(this.prefix) === -1
      ? `/trash?t=!delete!`
      : `${this.prefix}/trash?t=!delete!`;
  }

  public UrlImportPage(): string {
    return document.location.pathname.indexOf(this.prefix) === -1
      ? `/import`
      : `${this.prefix}/import`;
  }

  public UrlPreferencesPage(): string {
    return document.location.pathname.indexOf(this.prefix) === -1
      ? `/preferences`
      : `${this.prefix}/preferences`;
  }

  public UrlLoginPage(): string {
    return document.location.pathname.indexOf(this.prefix) === -1
      ? `/account/login`
      : `${this.prefix}/account/login`;
  }

  public UrlLoginApi(): string {
    return this.prefix + `/api/account/login`;
  }

  public UrlLogoutPage(returnUrl: string): string {
    return document.location.pathname.indexOf(this.prefix) === -1
      ? `/account/logout?ReturnUrl=${returnUrl}`
      : `${this.prefix}/account/logout?ReturnUrl=${returnUrl}`;
  }

  public UrlLogoutApi(): string {
    return this.prefix + `/api/account/logout`;
  }

  private urlReplacePath(input: string): string {
    let output = input.replace("#", "");
    return output.replace(/\+/gi, "%2B");
  }

  public UrlAccountRegisterApi(): string {
    return `${this.prefix}/api/account/register`;
  }

  public UrlAccountRegisterPage(): string {
    return `${this.prefix}/account/register`;
  }

  public UrlSearchRelativeApi = (
    f: string,
    t: string | undefined,
    pageNumber = 0
  ): string => {
    return (
      `${this.prefix}/api/search/relative-objects?f=` +
      new URLPath().encodeURI(f) +
      "&t=" +
      t +
      "&p=" +
      pageNumber
    );
  };

  public UrlQuerySearchApi = (query: string, pageNumber = 0): string => {
    return (
      this.prefix + "/api/search?json=true&t=" + query + "&p=" + pageNumber
    );
  };

  public UrlSearchSuggestApi(query: string): string {
    return this.prefix + "/api/suggest/?t=" + query;
  }

  public UrlSearchRemoveCacheApi(): string {
    return this.prefix + "/api/search/remove-cache";
  }

  public UrlSearchTrashApi = (pageNumber = 0): string => {
    return this.prefix + "/api/search/trash?p=" + pageNumber;
  };

  public UrlAccountStatus = (): string => {
    return this.prefix + "/api/account/status";
  };

  public UrlAccountRegisterStatus = (): string => {
    return this.prefix + "/api/account/register/status";
  };

  public UrlAccountChangeSecret = (): string => {
    return this.prefix + "/api/account/change-secret";
  };

  public UrlAccountPermissions = (): string => {
    return this.prefix + "/api/account/permissions";
  };

  /**
   * Keep colorClass in URL
   */
  public updateFilePathHash(
    historyLocationHash: string,
    toUpdateFilePath: string,
    clearTSearchQuery?: boolean,
    emthySelectQuery?: boolean
  ): string {
    const url = new URLPath().StringToIUrl(historyLocationHash);
    url.f = toUpdateFilePath;
    // when browsing to a parent folder from a detailview item
    if (clearTSearchQuery) {
      delete url.t;
      delete url.p;
    }
    // when in select mode and navigate next to the select mode is still on but there are no items selected
    if (emthySelectQuery && url.select && url.select?.length >= 1) {
      url.select = [];
    }
    return document.location.pathname.indexOf(this.prefix) === -1
      ? `/${new URLPath().IUrlToString(url)}`
      : `${this.prefix}/${new URLPath().IUrlToString(url)}`;
  }

  /**
   * Used with localisation hash
   */
  public UrlQueryServerApi = (historyLocationHash: string): string => {
    const requested = new URLPath().StringToIUrl(historyLocationHash);

    const urlObject = newIUrl();
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

    if (requested.sort) {
      urlObject.sort = requested.sort;
    }

    return this.UrlIndexServerApi(urlObject);
  };

  /**
   * Get Direct api/index with IUrl
   */
  public UrlIndexServerApi = (urlObject: IUrl): string => {
    return this.prefix + "/api/index" + new URLPath().IUrlToString(urlObject);
  };

  /**
   * Get Direct api/index with IUrl
   */
  public UrlIndexServerApiPath = (path: string): string => {
    return this.prefix + "/api/index?f=" + path;
  };

  /**
   * GET: Gets the realtime API
   * @param subPath subpath style
   */
  public UrlQueryInfoApi(subPath: string): string {
    if (!subPath) return "";
    const url = this.urlReplacePath(subPath);
    return this.prefix + "/api/info?f=" + url + "&json=true";
  }

  /**
   * POST to this to update meta information
   */
  public UrlUpdateApi = (): string => {
    return this.prefix + "/api/update";
  };

  /**
   * POST to trash this item
   */
  public UrlMoveToTrashApi = (): string => {
    return this.prefix + "/api/trash/move-to-trash";
  };

  /**
   * GET recent notifications
   */
  public UrlNotificationsGetApi = (keepAliveServerTime: string): string => {
    return (
      this.prefix +
      "/api/notification/notification?dateTime=" +
      keepAliveServerTime
    );
  };

  /**
   * POST to this to search and replace meta information like: tags, descriptions and titles
   */
  public UrlReplaceApi = (): string => {
    return this.prefix + "/api/replace";
  };

  /**
   * GET to coordinates
   * @returns url
   */
  public UrlReverseLookup = (latitude: string, longitude: string): string => {
    return `${this.prefix}/api/geo-reverse-lookup?latitude=${latitude}&longitude=${longitude}`;
  };

  /**
   * DELETE to endpoint to remove file from database and disk
   */
  public UrlDeleteApi = (): string => {
    return this.prefix + "/api/delete";
  };

  public UrlThumbnailImageLargeOrExtraLarge = (
    fileHash: string,
    filePath?: string,
    extraLarge = true
  ): string => {
    if (!extraLarge) {
      return (
        this.prefix +
        "/api/thumbnail/" +
        fileHash +
        ".jpg?issingleitem=true&extraLarge=false" +
        "&filePath=" +
        filePath
      );
    }
    return (
      this.prefix +
      "/api/thumbnail/" +
      fileHash +
      "@2000.jpg?issingleitem=true&extraLarge=true" +
      "&filePath=" +
      filePath
    );
  };

  public UrlThumbnailImage = (
    fileHash: string,
    alwaysLoadImage: boolean
  ): string => {
    if (alwaysLoadImage) {
      return (
        this.prefix + "/api/thumbnail/" + fileHash + ".jpg?issingleitem=true"
      );
    }
    return this.prefix + "/api/thumbnail/small/" + fileHash + ".jpg";
  };

  /**
   * /api/thumbnail/zoom/{f}@{z}
   * @param f filehash
   * @param z zoomfactor
   * @param id filePath
   */
  public UrlThumbnailZoom = (
    f: string,
    id: string | undefined,
    z: number
  ): string => {
    return `${this.prefix}/api/thumbnail/zoom/${f}@${z}?filePath=${id}`;
  };

  public UrlThumbnailJsonApi = (fileHash: string): string => {
    return this.prefix + "/api/thumbnail/" + fileHash + "?json=true";
  };

  public UrlDownloadPhotoApi = (
    f: string,
    isThumbnail: boolean = true,
    cache: boolean = true
  ): string => {
    return (
      this.prefix +
      "/api/download-photo?f=" +
      f +
      "&isThumbnail=" +
      isThumbnail +
      "&cache=" +
      cache
    );
  };

  public UrlApiAppSettings = (): string => {
    return this.prefix + "/api/env/";
  };

  public UrlApiFeaturesAppSettings = (): string => {
    return this.prefix + "/api/env/features";
  };

  /**
   * url create a zip
   */
  public UrlExportPostZipApi = (): string => {
    return this.prefix + "/api/export/create-zip/";
  };

  /**
   * export/zip/SR497519527.zip?json=true
   */
  public UrlExportZipApi = (
    createZipId: string,
    json: boolean = true
  ): string => {
    return this.prefix + "/api/export/zip/" + createZipId + ".zip?json=" + json;
  };

  /**
   * Url of Sync (no need to encode parentFolder before input)
   * @param parentFolder no need to encode this (done in this method)
   */
  public UrlSync(parentFolder: string): string {
    return (
      this.prefix +
      "/api/synchronize?f=" +
      new URLPath().encodeURI(parentFolder)
    );
  }

  /**
   * Rename the file on disk and in the database
   */
  public UrlDiskRename(): string {
    return this.prefix + "/api/disk/rename";
  }

  /**
   * Create an directory on disk and database
   */
  public UrlDiskMkdir(): string {
    return this.prefix + "/api/disk/mkdir";
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
    return `${this.prefix}/api/health/details`;
  }

  public UrlHealthCheckForUpdates(): string {
    return `${this.prefix}/api/health/check-for-updates`;
  }

  public UrlRemoveCache(parentFolder: string): string {
    return this.prefix + "/api/remove-cache?json=true&f=" + parentFolder;
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

  public UrlPublish(): string {
    return this.prefix + "/api/publish";
  }

  public UrlPublishCreate(): string {
    return this.prefix + "/api/publish/create";
  }

  public UrlPublishExist(itemName: string | null): string {
    return this.prefix + `/api/publish/exist?itemName=${itemName}`;
  }

  public UrlRealtime(): string {
    let url = window.location.protocol === "https:" ? "wss:" : "ws:";
    url += "//" + window.location.host + this.prefix + "/realtime";

    // Create React App is not supporting websockets. At least is isn't working
    if (isDev()) {
      url = url.replace(":3000", ":4000");
    }
    return url;
  }
}
