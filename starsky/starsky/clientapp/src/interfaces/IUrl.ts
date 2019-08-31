export interface IUrl {
  f?: string;
  details?: boolean,
  sidebar?: boolean,
  select?: Array<string>;
  colorClass?: Array<number>
}

export function newIUrl(): IUrl {
  return {} as IUrl;
}