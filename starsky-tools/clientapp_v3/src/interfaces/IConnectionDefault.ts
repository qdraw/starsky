export interface IConnectionDefault {
  data: any,
  statusCode: number
}
export function newIConnectionDefault(): IConnectionDefault {
  return {
    data: null,
    statusCode: 999
  } as IConnectionDefault;
}