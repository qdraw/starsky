
export interface IPreloadApi {
    send(string: string, any: any): void;
    receive(string: string, func: Function): void; 
  }