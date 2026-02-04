export interface IConnectionDefault {
  data: unknown;
  statusCode: number;
}
export function newIConnectionDefault(): IConnectionDefault {
  return {
    data: null,
    statusCode: 999
  } as IConnectionDefault;
}
