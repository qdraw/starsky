import { IRelativeObjects, PageType } from "./IDetailView";
import { IFileIndexItem } from "./IFileIndexItem";

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
}

export function newIArchive(): IArchive {
    return {} as IArchive;
}