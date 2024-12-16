import { SetStateAction, useEffect, useState } from "react";
import { IArchive, newIArchive } from "../interfaces/IArchive";
import { IDetailView, PageType, newDetailView } from "../interfaces/IDetailView";
import { CastToInterface } from "../shared/cast-to-interface";
import { FileListCache } from "../shared/filelist-cache";
import { URLPath } from "../shared/url/url-path";
import { UrlQuery } from "../shared/url/url-query";

export interface IFileList {
  archive?: IArchive;
  detailView?: IDetailView;
  pageType: PageType;
  parent: string;
  fetchUseFileListContentCache: (
    locationLocal: string,
    locationSearch: string,
    abortController: AbortController,
    setPageTypeHelper: (responseObject: any) => boolean,
    resetPageTypeBeforeLoading: boolean,
    setPageType: (value: SetStateAction<PageType>) => void
  ) => Promise<void>;
  setPageTypeHelper: (responseObject: any) => boolean;
}

export const fetchContentUseFileList = async (
  locationLocal: string,
  locationSearch: string,
  abortController: AbortController,
  setPageTypeHelper: (responseObject: any) => void,
  resetPageTypeBeforeLoading: boolean,
  setPageType: (value: SetStateAction<PageType>) => void
): Promise<void> => {
  try {
    // force start with a loading icon
    if (resetPageTypeBeforeLoading) setPageType(PageType.Loading);

    const res: Response = await fetch(locationLocal, {
      signal: abortController.signal,
      credentials: "include",
      method: "get"
    });

    if (res.status === 404) {
      setPageType(PageType.NotFound);
      return;
    } else if (res.status === 401) {
      setPageType(PageType.Unauthorized);
      return;
    } else if (res.status >= 400 && res.status <= 550) {
      setPageType(PageType.ApplicationException);
      return;
    }

    const responseObject = await res.json();
    setPageTypeHelper(responseObject);
    new FileListCache().CacheSet(locationSearch, responseObject);
  } catch (e: any) {
    if (e?.message?.indexOf("aborted") >= 0) {
      return;
    }
    console.error(e);
    setPageType(PageType.ApplicationException);
  }
};

const fetchUseFileListContentCache = async (
  locationLocal: string,
  locationSearch: string,
  abortController: AbortController,
  setPageTypeHelper: (responseObject: any) => boolean,
  resetPageTypeBeforeLoading: boolean,
  setPageType: (value: SetStateAction<PageType>) => void
): Promise<void> => {
  const content = new FileListCache().CacheGet(locationSearch);
  if (content) {
    console.log(
      ` -- Cache Content ${new Date(content.dateCache).toLocaleTimeString()} ${locationSearch} -- `
    );
    setPageTypeHelper(content);
  } else {
    console.log(` -- Fetch Content ${locationSearch} -- `);
    await fetchContentUseFileList(
      locationLocal,
      locationSearch,
      abortController,
      setPageTypeHelper,
      resetPageTypeBeforeLoading,
      setPageType
    );
  }
};

/**
 * Hook to get index API
 * @param locationSearch with query parameter "?f=/"
 * @param resetPageTypeBeforeLoading start direct with loading state = true is enable, use false to have smooth page transitions
 */
const useFileList = (
  locationSearch: string,
  resetPageTypeBeforeLoading: boolean
): IFileList | null => {
  const [archive, setArchive] = useState(newIArchive());
  const [detailView, setDetailView] = useState(newDetailView());
  const [pageType, setPageType] = useState(PageType.Loading);
  const [parent, setParent] = useState("/");
  const location = new UrlQuery().UrlQueryServerApi(locationSearch);

  const setPageTypeHelper = (responseObject: any): boolean => {
    setParent(new URLPath().getParent(locationSearch));

    if (
      !responseObject?.pageType ||
      responseObject?.pageType === PageType.NotFound ||
      responseObject?.pageType === PageType.ApplicationException
    ) {
      return false;
    }

    responseObject.sort = new URLPath().StringToIUrl(locationSearch).sort;
    setPageType(responseObject.pageType);
    switch (responseObject.pageType) {
      case PageType.Archive:
        setArchive(new CastToInterface().MediaArchive(responseObject).data);
        return true;
      case PageType.DetailView:
        setDetailView(new CastToInterface().MediaDetailView(responseObject).data);
        return true;
      default:
        break;
    }
    return false;
  };

  useEffect(() => {
    const abortController = new AbortController();
    fetchUseFileListContentCache(
      location,
      locationSearch,
      abortController,
      setPageTypeHelper,
      resetPageTypeBeforeLoading,
      setPageType
    ).then(() => {
      // do nothing
    });

    return () => {
      abortController.abort();
    };

    // dependency: 'locationSearch'. is not added to avoid a lot of queries
    // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
  }, [location]);

  return {
    archive,
    detailView,
    pageType,
    parent,
    fetchUseFileListContentCache,
    setPageTypeHelper
  };
};

export default useFileList;
