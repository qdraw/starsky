import { IRelativeObjects, PageType } from "./IDetailView";
import { IFileIndexItem } from "./IFileindexItem";

export interface IArchive {
  breadcrumb: Array<string>;
  pageType: PageType;
  fileIndexItems: Array<IFileIndexItem>;
  relativeObjects: IRelativeObjects;
  subPath: string;
  colorClassActiveList: Array<number>;
  colorClassUsage: Array<number>;
  collectionsCount: number;
  isReadOnly: boolean;
  searchQuery?: string;
  collections?: boolean;
  dateCache: number;
  sort?: SortType;
}

export enum SortType {
  fileName = "fileName",
  imageFormat = "imageFormat",
}

export function newIArchive(): IArchive {
  return {} as IArchive;
}
