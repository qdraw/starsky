import { IExifStatus } from './IExifStatus';
import { IFileIndexItem } from "./IFileIndexItem";

export enum PageType {
    Loading,
    Archive, // index
    DetailView,
    Search,
    ApplicationException,
    NotFound,
}
export interface IRelativeObjects {
    nextFilePath: string;
    prevFilePath: string;
    args: Array<string>;
}

export function newIRelativeObjects(): IRelativeObjects {
    return {
        nextFilePath: "",
        prevFilePath: "",
        args: new Array<string>(),
    } as IRelativeObjects;
}

export interface IDetailView {
    breadcrumb: [];
    pageType: string; // casting enums fails
    fileIndexItem: IFileIndexItem;
    relativeObjects: IRelativeObjects;
    subPath: string;
    status: IExifStatus | null;
    colorClassFilterList: Array<number>;
    lastUpdated?: Date;
}

export function newDetailView(): IDetailView {
    return {
        status: null
    } as IDetailView;
}