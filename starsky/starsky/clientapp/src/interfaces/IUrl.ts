export interface IUrl {
  f?: string;
  details?: boolean,
  sidebar?: boolean,
  select?: Array<string> | null;
  colorClass?: Array<number>
}

export function newIUrl(): IUrl {
  return {} as IUrl;
}