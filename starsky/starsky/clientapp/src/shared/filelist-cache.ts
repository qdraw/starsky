import { IArchive, SortType } from "../interfaces/IArchive";
import { IDetailView, PageType } from "../interfaces/IDetailView";
import { IUrl } from "../interfaces/IUrl";
import { DifferenceInDate } from "./date";
import { URLPath } from "./url-path";

interface IGetAllTransferObject {
  name: string;
  item: IDetailView | IArchive;
}

type NullableIArchiveOrDetailView = IArchive | IDetailView | null;

export class FileListCache {
  private cachePrefix = "starsky;";

  private timeoutInMinutes = 3;

  public CacheCleanByPath(path: string) {
    this.GetAll().forEach((item) => {
      if (item.name.includes(`f:${path}`)) {
        console.log("--dsfnkl");

        try {
          console.log(item.item);

          const updatedItem = item.item;
          updatedItem.dateCache = 1;
          sessionStorage.setItem(item.name, JSON.stringify(item.item));
        } catch (error) {
          console.log(error);
        }
      }
    });
  }

  /**
   * Set entire list of for a folder
   * @param urlObject params of url
   * @param value object with data
   */
  public CacheSetObject(urlObject: IUrl, value: IArchive | IDetailView): any {
    this.CacheSetObjectWithoutParent(urlObject, value);
    this.CacheCleanOld();
    this.setParentItem(urlObject, value);
    return value;
  }

  private SetDefaultUrlObjectValues(urlObject: IUrl): IUrl {
    if (!urlObject.f) urlObject.f = "/";
    if (urlObject.collections === undefined) urlObject.collections = true;
    if (!urlObject.colorClass) urlObject.colorClass = [];
    return urlObject;
  }

  private CacheSetObjectWithoutParent(
    urlObject: IUrl,
    value: IArchive | IDetailView
  ): any {
    if (localStorage.getItem("clientCache") === "false") return;
    urlObject = this.SetDefaultUrlObjectValues(urlObject);
    value.dateCache = Date.now();

    try {
      // old versions of safari don't allow sessionStorage in private navigation
      sessionStorage.setItem(
        this.CacheKeyGenerator(urlObject),
        JSON.stringify(value)
      );
      return value;
    } catch (error) {
      console.error(error);
    }
  }

  /**
   * Only updates the parent item with the same colorclass + collections indexer
   * @param urlObject where to look for
   * @param value the detailview object that contains parent path
   */
  private setParentItem(urlObject: IUrl, value: IDetailView | IArchive): void {
    if (!value.pageType || value.pageType !== PageType.DetailView) {
      return;
    }
    var detailview = value as IDetailView;
    if (!detailview.fileIndexItem) {
      return;
    }

    var parentItem = this.CacheGetObject({
      ...urlObject,
      f: detailview.fileIndexItem.parentDirectory
    }) as IArchive;
    if (!parentItem || parentItem.pageType !== PageType.Archive) {
      return;
    }

    parentItem.fileIndexItems.forEach((item, index) => {
      if (!urlObject.collections) {
        if (
          item.fileName &&
          item.fileName === detailview.fileIndexItem.fileName
        ) {
          parentItem.fileIndexItems[index] = detailview.fileIndexItem;
        }
      }
      if (urlObject.collections) {
        if (
          item.fileName &&
          item.fileName.startsWith(detailview.fileIndexItem.fileCollectionName)
        ) {
          parentItem.fileIndexItems[index] = detailview.fileIndexItem;
        }
      }
    });
    this.CacheSetObjectWithoutParent(
      { ...urlObject, f: detailview.fileIndexItem.parentDirectory },
      parentItem
    );
  }

  public CacheKeyGenerator(urlObject: IUrl) {
    return (
      this.cachePrefix +
      `c${urlObject.colorClass};l${urlObject.collections}` +
      // should have f:
      `f:${urlObject.f}` +
      `;s${!urlObject.sort ? SortType.fileName : urlObject.sort}`
    );
  }

  public CacheSet(locationSearch: string, value: IArchive | IDetailView): any {
    var urlObject = new URLPath().StringToIUrl(locationSearch);
    return this.CacheSetObject(urlObject, value);
  }

  /**
   * GETTER of cache
   * @param locationSearch where to look for
   */
  public CacheGet(locationSearch: string): NullableIArchiveOrDetailView {
    var urlObject = new URLPath().StringToIUrl(locationSearch);
    return this.CacheGetObject(urlObject);
  }

  /**
   * GETTER of cache
   * @param urlObject where to look for
   */
  public CacheGetObject(urlObject: IUrl): NullableIArchiveOrDetailView {
    if (localStorage.getItem("clientCache") === "false") return null;
    urlObject = this.SetDefaultUrlObjectValues(urlObject);

    var cache = this.ParseJson(
      sessionStorage.getItem(this.CacheKeyGenerator(urlObject))
    );
    if (!cache) return null;

    if (DifferenceInDate(cache.dateCache) > this.timeoutInMinutes) {
      return null;
    }
    return cache;
  }

  /**
   * Get all items from Session Storage
   */
  private GetAll(): IGetAllTransferObject[] {
    var list = [];
    for (const itemName of Object.keys(sessionStorage)) {
      if (!itemName || !itemName.startsWith(this.cachePrefix)) continue;
      var item = this.ParseJson(sessionStorage.getItem(itemName));
      if (!item || !item.dateCache) continue;
      list.push({
        name: itemName,
        item
      });
    }
    return list;
  }

  /**
   * And clean the old ones
   */
  public CacheCleanOld(): void {
    this.GetAll().forEach((item) => {
      if (DifferenceInDate(item.item.dateCache) > this.timeoutInMinutes) {
        sessionStorage.removeItem(item.name);
      }
    });
  }

  /**
   * And clean All Items
   */
  public CacheCleanEverything(): void {
    this.GetAll().forEach((item) => {
      sessionStorage.removeItem(item.name);
    });
  }

  /**
   * Parse Json from string
   * @param cacheString input
   */
  public ParseJson(cacheString: string | null): IArchive | IDetailView | null {
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
