import { INavigateState } from "../../../interfaces/INavigateState";

export interface ILocationObject {
  href: string;
  search: string;
  state?: INavigateState | undefined;
}
