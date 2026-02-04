import { Router } from "../../../router-app/router-app";
import { INavigateOptions } from "../interfaces/INavigatieoptions";

export function NavigateFn(to: string, options?: INavigateOptions): void {
  Router.navigate(to, {
    relative: "path",
    replace: options?.replace
  });
}
