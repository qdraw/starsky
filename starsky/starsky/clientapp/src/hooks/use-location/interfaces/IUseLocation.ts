import { INavigateState } from "../../../interfaces/INavigateState";
import { NavigateFunction } from "../type/NavigateFunction";
import { ILocationObject } from "./ILocationObject";

export interface IUseLocation {
  location: ILocationObject;
  navigate: NavigateFunction;
  state?: INavigateState;
}
