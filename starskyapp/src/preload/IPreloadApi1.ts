export interface IPreloadApi {
  send(string: string, any: any): void;
  // eslint-disable-next-line @typescript-eslint/ban-types
  receive(string: string, func: Function): void;
}
