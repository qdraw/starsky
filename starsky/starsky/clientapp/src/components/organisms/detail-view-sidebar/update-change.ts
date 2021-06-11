import { DetailViewAction } from "../../../contexts/detailview-context";
import { IUseLocation } from "../../../hooks/use-location";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { IDetailView } from "../../../interfaces/IDetailView";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { AsciiNull } from "../../../shared/ascii-null";
import FetchPost from "../../../shared/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { ClearSearchCache } from "../../../shared/search/clear-search-cache";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";

export class UpdateChange {
  private fileIndexItem: IFileIndexItem;
  private setFileIndexItem: (
    value: React.SetStateAction<IFileIndexItem>
  ) => void;
  private dispatch: React.Dispatch<DetailViewAction>;
  private history: IUseLocation;
  private state: IDetailView;

  constructor(
    fileIndexItem: IFileIndexItem,
    setFileIndexItem: (value: React.SetStateAction<IFileIndexItem>) => void,
    dispatch: React.Dispatch<DetailViewAction>,
    history: IUseLocation,
    state: IDetailView
  ) {
    this.fileIndexItem = fileIndexItem;
    this.setFileIndexItem = setFileIndexItem;
    this.dispatch = dispatch;
    this.history = history;
    this.state = state;
    // bind this to object
    this.Update = this.Update.bind(this);
  }

  /**
   * Send update request
   * @param items - tuple with "value: string, name: string"
   */
  public Update(items: [string, string][]): Promise<string | boolean> {
    const updateObject: any = { f: this.fileIndexItem.filePath };

    for (let [name, value] of items) {
      if (!name) continue;

      let replacedValue = value.replace(AsciiNull(), "");
      // allow empty requests
      if (!replacedValue) replacedValue = AsciiNull();

      // compare
      const fileIndexObject: any = this.fileIndexItem;

      if (!fileIndexObject[name] === undefined) continue; //to update empty start to first fill

      const currentString: string = fileIndexObject[name];
      if (replacedValue === currentString) continue;

      updateObject[name] = replacedValue.trim();
    }

    const bodyParams = new URLPath()
      .ObjectToSearchParams(updateObject)
      .toString()
      .replace(/%00/gi, AsciiNull());

    if (bodyParams === "") return Promise.resolve("no body param");

    return new Promise((resolve) => {
      FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams)
        .then((item) => {
          this.AfterPost(item, resolve);
        })
        .catch((e) => resolve(e));
    });
  }

  public AfterPost(
    item: IConnectionDefault,
    resolve: (value: string | boolean | PromiseLike<string | boolean>) => void
  ) {
    if (item.statusCode !== 200 || !item.data || item.data.length === 0) {
      resolve("wrong status code or missing data");
      return;
    }

    // the order of the item list is by alphabet
    const currentItemList = (item.data as IFileIndexItem[]).filter(
      (p) => p.filePath === this.fileIndexItem.filePath
    );

    if (currentItemList.length === 0) {
      resolve("item not in result");
      return;
    }

    const currentItem = currentItemList[0];
    currentItem.lastEdited = new Date().toISOString();

    this.setFileIndexItem(currentItem);
    this.dispatch({ type: "update", ...currentItem });
    ClearSearchCache(this.history.location.search);
    new FileListCache().CacheSet(this.history.location.search, {
      ...this.state,
      fileIndexItem: currentItem
    });
    resolve(true);
  }
}
