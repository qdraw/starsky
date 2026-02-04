import { IPreloadApi } from "../../preload/IPreloadApi";
import { warmupLocalOrRemote } from "./reload-warmup-local-or-remote";

declare global {
  // eslint-disable-next-line vars-on-top, no-var
  var api: IPreloadApi;
}

warmupLocalOrRemote();
