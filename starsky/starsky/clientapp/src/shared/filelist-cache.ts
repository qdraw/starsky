import { IArchive } from '../interfaces/IArchive';
import { IDetailView } from '../interfaces/IDetailView';
import { IUrl } from '../interfaces/IUrl';
import { DifferenceInDate } from './date';
import { URLPath } from './url-path';

export class FileListCache {

  constructor() {
    if (sessionStorage) return;
    throw Error("Session Storage is needed")
  }

  private cachePrefix = "starsky;";

  private timeoutInMinutes = 3;

  public CacheSetObject(urlObject: IUrl, value: any): any {
    value.dateCache = Date.now();
    sessionStorage.setItem(this.CacheKeyGenerator(urlObject), JSON.stringify(value));

    this.CacheCleanOld();
    return value;
  }

  public CacheKeyGenerator(urlObject: IUrl) {
    return this.cachePrefix +
      `c${urlObject.colorClass};l${urlObject.collections}` + urlObject.f;
  }

  public CacheSet(locationSearch: string, value: any): any {
    var urlObject = new URLPath().StringToIUrl(locationSearch);
    if (!urlObject.f) urlObject.f = "/";
    if (urlObject.collections === undefined) urlObject.collections = true;
    return this.CacheSetObject(urlObject, value);
  }

  public CacheGet(locationSearch: string): IArchive | IDetailView | null {
    var urlObject = new URLPath().StringToIUrl(locationSearch);
    if (!urlObject.f) urlObject.f = "/";
    if (urlObject.collections === undefined) urlObject.collections = true;
    return this.CacheGetObject(urlObject);
  }

  public CacheGetObject(urlObject: IUrl): IArchive | IDetailView | null {
    var cache = this.parseJson(sessionStorage.getItem(this.CacheKeyGenerator(urlObject)))
    if (!cache) return null;

    if (DifferenceInDate(cache.dateCache) > this.timeoutInMinutes) {
      return null;
    }
    return cache;
  }

  /**
   * And clean the old ones
   */
  public CacheCleanOld(): void {
    for (let index = 0; index < Object.keys(sessionStorage).length; index++) {
      const itemName = Object.keys(sessionStorage)[index];
      if (!itemName || !itemName.startsWith(this.cachePrefix)) continue;
      var item = this.parseJson(sessionStorage.getItem(itemName));
      if (!item || !item.dateCache) continue;
      if (DifferenceInDate(item.dateCache) > this.timeoutInMinutes) {
        sessionStorage.removeItem(itemName)
      }
    }
  }

  private parseJson(cacheString: string | null): IArchive | IDetailView | null {
    if (!cacheString) return null;
    var cacheData: any = {};
    try {
      cacheData = JSON.parse(cacheString);
    } catch (error) {
      console.error(error);
      return null;
    }
    return cacheData.dateCache ? cacheData : null;
  }
}