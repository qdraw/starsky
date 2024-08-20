import { IFileIndexItem } from "./IFileindexItem";

export enum PageType {
  Loading = "Loading",
  Archive = "Archive",
  DetailView = "DetailView",
  Search = "Search",
  ApplicationException = "ApplicationException",
  NotFound = "NotFound",
  Unauthorized = "Unauthorized",
  Trash = "Trash",
}

export interface IRelativeObjects {
  nextFilePath: string;
  prevFilePath: string;
  nextHash: string;
  prevHash: string;
  args: Array<string>;
}

export function newIRelativeObjects(): IRelativeObjects {
  return {
    nextFilePath: "",
    prevFilePath: "",
    nextHash: "",
    prevHash: "",
    args: new Array<string>(),
  } as IRelativeObjects;
}

export interface IDetailView {
  breadcrumb: [];
  pageType: PageType;
  fileIndexItem: IFileIndexItem;
  relativeObjects: IRelativeObjects;
  subPath: string;
  colorClassActiveList: Array<number>;
  lastUpdated?: Date;
  isReadOnly: boolean;
  collections?: boolean;
  dateCache: number;
}

export function newDetailView(): IDetailView {
  return { pageType: PageType.DetailView } as IDetailView;
}
