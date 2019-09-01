export interface IUrl {
  f?: string; // filenames
  t?: string; // used for search
  p?: number; // pagination
  details?: boolean,
  sidebar?: boolean,
  select?: Array<string>;
  colorClass?: Array<number>
}

export function newIUrl(): IUrl {
  return {} as IUrl;
}