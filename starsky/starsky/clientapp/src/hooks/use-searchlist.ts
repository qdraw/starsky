import { Dispatch, SetStateAction, useEffect, useState } from "react";
import { IArchive, newIArchive } from "../interfaces/IArchive";
import { PageType } from "../interfaces/IDetailView";
import { CastToInterface } from "../shared/cast-to-interface";
import { UrlQuery } from "../shared/url-query";

export interface ISearchList {
  archive?: IArchive;
  pageType: PageType;
  fetchContent: (
    location: string,
    abortController: AbortController
  ) => Promise<void>;
}

/**
 * Fail on 401, 404 and other errors
 * @param res response
 * @param setPageType pagetype
 * @param setArchive when 401
 * @returns true when fail, false when success
 */
const setErrorPageType = (
  res: Response,
  setPageType: Dispatch<SetStateAction<PageType>>,
  setArchive: Dispatch<SetStateAction<IArchive>>
): boolean => {
  if (res.status === 404) {
    setPageType(PageType.NotFound);
    return true;
  } else if (res.status === 401) {
    setArchive({
      ...newIArchive(),
      pageType: PageType.Unauthorized,
      fileIndexItems: [],
      colorClassUsage: []
    });
    setPageType(PageType.Unauthorized);
    return true;
  } else if (res.status >= 400 && res.status <= 550) {
    setPageType(PageType.ApplicationException);
    return true;
  }
  return false;
};

const useSearchList = (
  query: string | undefined,
  pageNumber: number | undefined,
  resetPageTypeBeforeLoading: boolean
): ISearchList | null => {
  if (!pageNumber) pageNumber = 0;

  const [archive, setArchive] = useState(newIArchive());
  const [pageType, setPageType] = useState(PageType.Loading);

  var location = query
    ? new UrlQuery().UrlQuerySearchApi(query, pageNumber)
    : undefined;

  const fetchContent = async (
    locationScoped: string | undefined,
    abortController: AbortController
  ): Promise<void> => {
    try {
      if (!locationScoped) {
        setArchive({
          ...newIArchive(),
          pageType: PageType.Search,
          fileIndexItems: [],
          colorClassUsage: [],
          searchQuery: ""
        });
        setPageType(PageType.Search);
        return;
      }

      // force start with a loading icon
      if (resetPageTypeBeforeLoading) setPageType(PageType.Loading);

      const res: Response = await fetch(locationScoped, {
        signal: abortController.signal,
        credentials: "include",
        method: "GET"
      });

      // 401, 404 and other errors
      if (setErrorPageType(res, setPageType, setArchive)) {
        return;
      }

      const responseObject = await res.json();

      var archiveMedia = new CastToInterface().MediaArchive(responseObject);
      setPageType(archiveMedia.data.pageType);

      if (
        archiveMedia.data.pageType !== PageType.Search &&
        archiveMedia.data.pageType !== PageType.Trash
      )
        return;

      // We don't know those values in the search context
      archiveMedia.data.colorClassUsage = [];
      archiveMedia.data.colorClassActiveList = [];
      setArchive(archiveMedia.data);
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    const abortController = new AbortController();
    fetchContent(location, abortController);

    return () => {
      abortController.abort();
    };

    // dependency: 'locationSearch'. is not added to avoid a lot of queries
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location]);

  return {
    archive,
    pageType,
    fetchContent
  };
};

export default useSearchList;
