import { ILocationObject } from "./ILocationObject";
import { INavigateFunction } from "./INavigateFunction";

export interface IUseLocation {
  location: ILocationObject;
  navigate: INavigateFunction;
}
