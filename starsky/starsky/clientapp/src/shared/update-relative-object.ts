import { IDetailView, IRelativeObjects } from "../interfaces/IDetailView";
import FetchGet from "./fetch/fetch-get";
import { URLPath } from "./url/url-path";
import { UrlQuery } from "./url/url-query";

export class UpdateRelativeObject {
  /**
   * UrlSearchRelativeApi using FetchGet
   */
  public Update(
    state: IDetailView,
    isSearchQuery: boolean,
    historyLocationSearch: string,
    setRelativeObjects: React.Dispatch<React.SetStateAction<IRelativeObjects>>
  ): Promise<IRelativeObjects> {
    return new Promise((resolve, rejects) => {
      if (state.subPath === "/" || !isSearchQuery) {
        rejects(new Error("no subpath or query"));
        return;
      }

      FetchGet(
        new UrlQuery().UrlSearchRelativeApi(
          state.subPath,
          new URLPath().StringToIUrl(historyLocationSearch).t,
          new URLPath().StringToIUrl(historyLocationSearch).p
        )
      )
        .then((result) => {
          if (result.statusCode !== 200) {
            rejects(new Error("status code not 200"));
            return;
          }
          setRelativeObjects(result.data as React.SetStateAction<IRelativeObjects>);
          resolve(result.data as IRelativeObjects);
        })
        .catch((err: Error) => {
          console.error(err);
          rejects(err);
        });
    });
  }
}
