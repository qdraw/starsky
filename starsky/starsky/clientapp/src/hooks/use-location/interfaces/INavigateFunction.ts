import { INavigateOptions } from "./INavigatieoptions";

export interface INavigateFunction {
  (to: string, options?: INavigateOptions): void;
}
