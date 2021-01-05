import { DetailViewAction } from "../../../contexts/detailview-context";
import { IUseLocation } from "../../../hooks/use-location";
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
    console.log("--ctor");

    this.fileIndexItem = fileIndexItem;
    this.setFileIndexItem = setFileIndexItem;
    this.dispatch = dispatch;
    this.history = history;
    this.state = state;

    console.log(this);
    this.Update = this.Update.bind(this);
  }

  // value: string, name: string
  public Update(items: [string, string][]) {
    const updateObject: any = { f: this.fileIndexItem.filePath };

    for (let [name, value] of items) {
      if (!name) continue;

      value = value.replace(AsciiNull(), "");
      // allow empty requests
      if (!value) value = AsciiNull();

      // compare
      const fileIndexObject: any = this.fileIndexItem;

      if (!fileIndexObject[name] === undefined) return; //to update empty start to first fill

      const currentString: string = fileIndexObject[name];
      if (value === currentString) continue;

      updateObject[name] = value.trim();
    }

    const bodyParams = new URLPath()
      .ObjectToSearchParams(updateObject)
      .toString()
      .replace(/%00/gi, AsciiNull());

    FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams).then((item) => {
      if (item.statusCode !== 200 || !item.data) return;

      var currentItem = item.data[0] as IFileIndexItem;
      currentItem.lastEdited = new Date().toISOString();

      this.setFileIndexItem(currentItem);
      this.dispatch({ type: "update", ...currentItem });
      ClearSearchCache(this.history.location.search);
      new FileListCache().CacheSet(this.history.location.search, {
        ...this.state,
        fileIndexItem: currentItem
      });
    });
  }
}
