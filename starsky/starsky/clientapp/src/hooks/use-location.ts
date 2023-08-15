import { INavigateState } from "../interfaces/INavigateState";
import { Router } from "../router-app/router-app";

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
  Router.navigate(to, {
    relative: "path",
    replace: options?.replace
  });
}

const useLocation = () => {
  console.log(Router.state.location);

  const result = {
    navigate: navigateFn,
    location: {
      ...Router.state.location,
      href:
        Router.state.location.pathname +
        Router.state.location.search +
        Router.state.location.hash
    }
  } as IUseLocation;

  return result;
};

export default useLocation;
