import { IPreloadApi } from "../../preload/IPreloadApi";
import { warmupLocalOrRemote } from "./reload-warmup-local-or-remote";
declare global {
  var api: IPreloadApi;
}

warmupLocalOrRemote();
