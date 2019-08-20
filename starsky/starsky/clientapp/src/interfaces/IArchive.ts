import { IRelativeObjects } from "./IDetailView";
import { IFileIndexItem } from "./IFileIndexItem";

export interface IArchive {
    breadcrumb: Array<string>;
    pageType: string; // casting enums fails
    fileIndexItems: Array<IFileIndexItem>;
    relativeObjects: IRelativeObjects;
    subPath: string;
    colorClassFilterList: Array<number>;
    colorClassUsage: Array<number>;
    collectionsCount: number;
}

export function newIArchive(): IArchive {
    return <IArchive>{};
}