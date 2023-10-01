import { ILocationObject } from "./ILocationObject";
import { NavigateFunction } from "../type/NavigateFunction";

export interface IUseLocation {
  location: ILocationObject;
  navigate: NavigateFunction;
}
