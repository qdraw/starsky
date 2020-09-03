import { IArchive } from '../interfaces/IArchive';
import { IDetailView } from '../interfaces/IDetailView';
import { IUrl } from '../interfaces/IUrl';
import { DifferenceInDate } from './date';
import { URLPath } from './url-path';

export class FileListCache {

  private cachePrefix = "starsky;";

  private timeoutInMinutes = 3;

  public CacheSetObject = (urlObject: IUrl, value: any): any => {
    if (!sessionStorage) return;

    value.dateCache = Date.now();
    sessionStorage.setItem(this.cachePrefix +
      `c${urlObject.colorClass};l${urlObject.collections}` + urlObject.f, JSON.stringify(value));

    this.CacheCleanOld();
    return value;
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

  public CacheGetObject = (urlObject: IUrl): IArchive | IDetailView | null => {
    if (!sessionStorage) return null;

    var cache = this.parseJson(sessionStorage.getItem(this.cachePrefix +
      `c${urlObject.colorClass};l${urlObject.collections}` + urlObject.f))
    if (!cache) return null;

    if (DifferenceInDate(cache.dateCache) > this.timeoutInMinutes) {
      return null;
    }
    return cache;
  }

  /**
   * And clean the old ones
   */
  public CacheCleanOld = (): void => {
    if (!sessionStorage) return;

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
      return null;
    }
    return cacheData.dateCache ? cacheData : null;
  }


}