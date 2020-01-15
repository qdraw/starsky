import { useEffect, useState } from 'react';
import { IArchive, newIArchive } from '../interfaces/IArchive';
import { PageType } from '../interfaces/IDetailView';
import { CastToInterface } from '../shared/cast-to-interface';
import { UrlQuery } from '../shared/url-query';

export interface ISearchList {
  archive?: IArchive,
  pageType: PageType,
}

const useSearchList = (query: string | undefined, pageNumber = 0, resetPageTypeBeforeLoading: boolean): ISearchList | null => {

  const [archive, setArchive] = useState(newIArchive());
  const [pageType, setPageType] = useState(PageType.Loading);

  var location = query ? new UrlQuery().UrlQuerySearchApi(query, pageNumber) : undefined;

  useEffect(() => {
    const abortController = new AbortController();

    (async () => {
      try {

        if (!location) {
          setArchive({ pageType: PageType.Search, ...newIArchive(), fileIndexItems: [], colorClassUsage: [], searchQuery: '' });
          setPageType(PageType.Search);
          return;
        }

        // force start with a loading icon 
        if (resetPageTypeBeforeLoading) setPageType(PageType.Loading);

        const res: Response = await fetch(location, {
          signal: abortController.signal,
          credentials: "include",
          method: 'GET'
        });

        if (res.status === 404) {
          setPageType(PageType.NotFound);
          return;
        }
        else if (res.status === 401) {
          setArchive({ pageType: PageType.Unauthorized, ...newIArchive(), fileIndexItems: [], colorClassUsage: [] });
          setPageType(PageType.Unauthorized);
          return;
        }
        else if (res.status >= 400 && res.status <= 550) {
          setPageType(PageType.ApplicationException);
          return;
        }

        const responseObject = await res.json();

        var archiveMedia = new CastToInterface().MediaArchive(responseObject);
        setPageType(archiveMedia.data.pageType);

        if (archiveMedia.data.pageType !== PageType.Search) return;

        // We don't know those values in the search context
        archiveMedia.data.colorClassUsage = [];
        archiveMedia.data.colorClassFilterList = [];
        setArchive(archiveMedia.data);

      } catch (e) {
        console.error(e);
      }
    })();

    return () => {
      abortController.abort();
    };
  }, [location]);

  return {
    archive,
    pageType,
  };
};

export default useSearchList;