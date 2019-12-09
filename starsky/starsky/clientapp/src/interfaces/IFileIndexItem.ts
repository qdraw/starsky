import { IExifStatus } from './IExifStatus';

export enum Color {
    Winner = 1, // Paars - purple
    WinnerAlt = 2, // rood - Red -
    Superior = 3, // Oranje - orange
    SuperiorAlt = 4, // Geel - yellow
    Typical = 5, // Groen - groen
    TypicalAlt = 6, // Turquoise
    Extras = 7, // Blauw - blue
    Trash = 8, // grijs - Grey
    None = 0, // donkergrijs Dark Grey
    DoNotChange = -1
}

export interface IFileIndexItem {
    lastEdited?: string;
    filePath: string;
    fileName: string;
    fileHash: string;
    parentDirectory: string;
    keywords?: [];
    status: IExifStatus;
    description?: string;
    isDirectory?: boolean;
    title?: string;
    dateTime?: string;
    tags?: string;
    latitude?: number;
    longitude?: number;
    colorClass?: number;
    orientation?: Orientation;
    imageFormat?: string;
    make?: string;
    model?: string;
    aperture?: number;
    isoSpeed?: number;
    shutterSpeed?: string;
    focalLength?: number;
    locationCountry?: string;
    locationCity?: string;
    imageWidth: number;
    imageHeight: number;
}

export enum Orientation {
    Horizontal = "Horizontal" as any,
    Rotate90Cw = "Rotate90Cw" as any,
    Rotate180 = "Rotate180" as any,
    Rotate270Cw = "Rotate270Cw" as any
}



// Warning: Input elements should not switch from uncontrolled to controlled https://fb.me/react-controlled-components
export function newIFileIndexItem(): IFileIndexItem {
    return {
        tags: "",
        title: "",
        description: "",
    } as IFileIndexItem;
}

export function newIFileIndexItemArray(): Array<IFileIndexItem> {
    return [];
}
