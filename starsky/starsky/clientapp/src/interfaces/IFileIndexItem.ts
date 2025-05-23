import { IExifStatus } from "./IExifStatus";

export interface IFileIndexItem {
  lastEdited?: string;
  lastChanged?: string[];
  filePath: string;
  fileName: string;
  fileCollectionName: string;
  fileHash: string;
  parentDirectory: string;
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
  imageFormat?: ImageFormat;
  make?: string;
  model?: string;
  lensModel?: string;
  aperture?: number;
  isoSpeed?: number;
  shutterSpeed?: string;
  focalLength?: number;
  locationCountry?: string;
  locationCountryCode?: string;
  locationCity?: string;
  locationState?: string;
  imageWidth?: number;
  imageHeight?: number;
  size?: number;
  sidecarExtensionsList?: string[];
  collectionPaths?: string[];
}

export enum ImageFormat {
  notfound = "notfound",
  unknown = "unknown",
  jpg = "jpg",
  tiff = "tiff",
  bmp = "bmp",
  gif = "gif",
  png = "png",
  webp = "webp",
  psd = "psd",
  xmp = "xmp",
  meta_json = "meta_json",
  gpx = "gpx",
  mp4 = "mp4",
  mjpeg = "mjpeg",
  mts = "mts"
}

export enum Orientation {
  Horizontal = "Horizontal",
  Rotate90Cw = "Rotate90Cw",
  Rotate180 = "Rotate180",
  Rotate270Cw = "Rotate270Cw"
}

// Warning: Input elements should not switch from uncontrolled to controlled https://fb.me/react-controlled-components
export function newIFileIndexItem(): IFileIndexItem {
  return {
    tags: "",
    title: "",
    description: ""
  } as IFileIndexItem;
}

export function newIFileIndexItemArray(): Array<IFileIndexItem> {
  return [];
}
