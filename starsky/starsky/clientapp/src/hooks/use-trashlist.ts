import { useEffect, useState } from 'react';
import { IArchive, newIArchive } from '../interfaces/IArchive';
import { PageType } from '../interfaces/IDetailView';
import { CastToInterface } from '../shared/cast-to-interface';
import { UrlQuery } from '../shared/url-query';

export interface IUseTrashList {
  archive?: IArchive,
  pageType: PageType,
}

const useTrashList = (pageNumber = 0): IUseTrashList | null => {

  const [archive, setArchive] = useState(newIArchive());
  const [pageType, setPageType] = useState(PageType.Loading);

  var location = new UrlQuery().UrlSearchTrashApi(pageNumber)

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


        if (res.status >= 400 && res.status <= 550) {
          setPageType(PageType.ApplicationException);
          return;
        }

        const responseObject = await res.json();

        var pageType = new CastToInterface().getPageType(responseObject);
        if (pageType === PageType.NotFound || pageType === PageType.ApplicationException) return;
        setPageType(pageType);

        if (pageType !== PageType.Search && pageType !== PageType.Trash) return;
        var archiveMedia = new CastToInterface().MediaArchive(responseObject);

        // for trash
        archiveMedia.data.pageType = pageType;
        // We don't know those values in the search context
        archiveMedia.data.colorClassUsage = [];
        archiveMedia.data.colorClassFilterList = [];
        setArchive(archiveMedia.data);

      } catch (e) {
        console.error(e);
      }
    })();

    const cleanup = () => {
      abortController.abort();
    };
    return cleanup;
  }, [location]);

  return {
    archive,
    pageType,
  };
};

export default useTrashList;