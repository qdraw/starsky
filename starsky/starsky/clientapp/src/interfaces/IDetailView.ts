import { IExifStatus } from './IExifStatus';
import { IFileIndexItem } from "./IFileIndexItem";

export enum PageType {
    Loading = "Loading" as any,
    Archive = "Archive" as any,
    DetailView = "DetailView" as any,
    Search = "Search" as any,
    ApplicationException = "ApplicationException" as any,
    NotFound = "NotFound" as any,
    Unauthorized = "Unauthorized" as any,
}


export interface IRelativeObjects {
    nextFilePath: string;
    prevFilePath: string;
    nextHash: string,
    prevHash: string,
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