import { INavigateState } from "../interfaces/INavigateState";
import history from "../shared/global-history/global-history";

export interface INavigateOptions {
  replace?: boolean;
  state?: INavigateState;
}

export interface ILocationObject {
  href: string;
  search: string;
  state: INavigateState;
}

export interface IUseLocation {
  location: ILocationObject;
  navigate: INavigateFunction;
}
export interface INavigateFunction {
  (to: string, options?: INavigateOptions): void;
}

function navigateFn(to: string, options?: INavigateOptions) {
  if (options?.replace === true) {
    history.replace(to, options.state);
    return;
  }
  history.push(to, options?.state);
}

const useLocation = () => {
  const result = {
    navigate: navigateFn,
    location: history.location as unknown as ILocationObject
  } as IUseLocation;

  return result;
};

export default useLocation;
