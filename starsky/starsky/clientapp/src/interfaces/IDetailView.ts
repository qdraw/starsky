import { IFileIndexItem } from "./IFileIndexItem";

export enum PageType {
    Loading = "Loading" as any,
    Archive = "Archive" as any,
    DetailView = "DetailView" as any,
    Search = "Search" as any,
    ApplicationException = "ApplicationException" as any,
    NotFound = "NotFound" as any,
    Unauthorized = "Unauthorized" as any,
    Trash = "Trash" as any,
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
    return {
    } as IDetailView;
}
