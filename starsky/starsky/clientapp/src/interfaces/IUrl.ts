import { SortType } from "./IArchive";

export interface IUrl {
  f?: string; // filenames
  t?: string; // used for search
  imageFormat?: string;
  camera?: string;
  keywords?: Array<string>;
  dateFrom?: string;
  dateTo?: string;
  filtersOpen?: boolean;
  p?: number; // pagination
  details?: boolean;
  sidebar?: boolean;
  select?: Array<string>;
  colorClass?: Array<number>;
  collections?: boolean;
  sort?: SortType;
  list?: boolean;
}

export function newIUrl(): IUrl {
  return {} as IUrl;
}
