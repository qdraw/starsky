import { useEffect, useState } from 'react';
import { IArchive, newIArchive } from '../interfaces/IArchive';
import { PageType } from '../interfaces/IDetailView';
import { CastToInterface } from '../shared/cast-to-interface';
import { Query } from '../shared/query';

export interface ISearchList {
  archive?: IArchive,
  // detailView?: IDetailView,
  pageType: PageType,
  // parent: string,
}

const useSearchList = (query: string | undefined, pageNumber = 0): ISearchList | null => {

  const [archive, setArchive] = useState(newIArchive());
  // const [detailView, setDetailView] = useState(newDetailView());
  const [pageType, setPageType] = useState(PageType.Loading);
  // const [parent, setParent] = useState('/');

  var location = query ? new Query().UrlQuerySearchApi(query, pageNumber) : undefined;

  useEffect(() => {
    const abortController = new AbortController();

    (async () => {
      try {

        if (!location) {
          setPageType(PageType.NotFound);
          return;
        }

        const res: Response = await fetch(location, {
          signal: abortController.signal,
          credentials: "include",
          method: 'GET'
        });

        if (res.status === 404) {
          setPageType(PageType.NotFound);
          return;
        }
        else if (res.status >= 400 && res.status <= 550) {
          setPageType(PageType.ApplicationException);
          return;
        }

        const responseObject = await res.json();
        console.log(responseObject);

        // setParent(new URLPath().getParent(locationSearch));

        var pageType = new CastToInterface().getPageType(responseObject);
        if (pageType === PageType.NotFound || pageType === PageType.ApplicationException) return;

        setPageType(pageType);
        switch (pageType) {
          case PageType.Search:
            var archiveMedia = new CastToInterface().MediaArchive(responseObject);
            // We don't know those values in the search context
            archiveMedia.data.colorClassUsage = [];
            archiveMedia.data.colorClassFilterList = [];
            setArchive(archiveMedia.data);
            break;
          default:
            break;
        }

      } catch (e) {
        console.error(e);
      }
    })();

    const cleanup = () => {
      abortController.abort();
    };
    return cleanup;
  }, [location]);
  // detailView,
  // parent

  return {
    archive,
    pageType,
  };
};

export default useSearchList;