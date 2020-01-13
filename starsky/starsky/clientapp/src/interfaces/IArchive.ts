import { IRelativeObjects, PageType } from "./IDetailView";
import { IFileIndexItem } from "./IFileIndexItem";

export interface IArchive {
    breadcrumb: Array<string>;
    pageType: PageType;
    fileIndexItems: Array<IFileIndexItem>;
    relativeObjects: IRelativeObjects;
    subPath: string;
    colorClassFilterList: Array<number>;
    colorClassUsage: Array<number>;
    collectionsCount: number;
    isReadOnly: boolean;
    searchQuery?: string;
}

export function newIArchive(): IArchive {
    return {} as IArchive;
}