import { IRelativeObjects, PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';

export interface IArchiveProps {
  fileIndexItems: Array<IFileIndexItem>;
  relativeObjects: IRelativeObjects;
  subPath: string;
  breadcrumb: Array<string>;
  colorClassActiveList: Array<number>;
  colorClassUsage: Array<number>;
  collectionsCount: number;
  lastUpdated?: Date; // used to trigger in context updates
  pageType: PageType; // Search or Archive
  pageNumber?: number;
  lastPageNumber?: number;
  isReadOnly: boolean;
  searchQuery?: string;
  collections?: boolean;
  dateCache: number;
}