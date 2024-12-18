import { Dispatch, SetStateAction } from "react";
import { ArchiveAction } from "../../../../contexts/archive-context.tsx";
import { IArchiveProps } from "../../../../interfaces/IArchiveProps.ts";
import { CastToInterface } from "../../../../shared/cast-to-interface.ts";
import { CacheControl } from "../../../../shared/fetch/cache-control.ts";
import FetchGet from "../../../../shared/fetch/fetch-get.ts";
import { FileListCache } from "../../../../shared/filelist-cache.ts";
import { URLPath } from "../../../../shared/url/url-path.ts";
import { UrlQuery } from "../../../../shared/url/url-query.ts";

/**
 * Remove Folder cache
 */
export function RemoveCache(
  setIsLoading: Dispatch<SetStateAction<boolean>>,
  propsParentFolder: string,
  historyLocationSearch: string,
  dispatch: Dispatch<ArchiveAction>,
  propsHandleExit: () => void
) {
  setIsLoading(true);
  new FileListCache().CacheCleanEverything();
  const parentFolder = propsParentFolder ?? "/";
  FetchGet(new UrlQuery().UrlRemoveCache(new URLPath().encodeURI(parentFolder)), CacheControl)
    .then(() => {
      return new Promise<string>((resolve) => {
        setTimeout(() => {
          resolve(
            new UrlQuery().UrlIndexServerApi(new URLPath().StringToIUrl(historyLocationSearch))
          );
        }, 600);
      });
    })
    .then((url: string) => {
      return FetchGet(url, CacheControl);
    })
    .then((connectionResult) => {
      const removeCacheResult = new CastToInterface().MediaArchive(connectionResult.data);
      const payload = removeCacheResult.data as IArchiveProps;
      if (payload.fileIndexItems) {
        dispatch({ type: "force-reset", payload });
      }
      propsHandleExit();
    })
    .catch((error) => {
      console.error("Error:", error);
    });
}
