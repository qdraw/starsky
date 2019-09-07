import { useEffect, useState } from 'react';
import { IArchive, newIArchive } from '../interfaces/IArchive';
import { IDetailView, newDetailView, PageType } from '../interfaces/IDetailView';
import { CastToInterface } from '../shared/cast-to-interface';
import { Query } from '../shared/query';
import { URLPath } from '../shared/url-path';

export interface IFileList {
  archive?: IArchive,
  detailView?: IDetailView,
  pageType: PageType,
  parent: string,
}

const useFileList = (locationSearch: string): IFileList | null => {

  const [archive, setArchive] = useState(newIArchive());
  const [detailView, setDetailView] = useState(newDetailView());
  const [pageType, setPageType] = useState(PageType.Loading);
  const [parent, setParent] = useState('/');

  var location = new Query().UrlQueryServerApi(locationSearch);

  useEffect(() => {
    const abortController = new AbortController();

    (async () => {
      try {
        const res: Response = await fetch(location, {
          signal: abortController.signal,
          credentials: "include",
          method: 'get'
        });

        if (res.status === 404) {
          setPageType(PageType.NotFound);
          return;
        }
        else if (res.status === 401) {
          setPageType(PageType.Unauthorized);
          return;
        }
        else if (res.status >= 400 && res.status <= 550) {
          setPageType(PageType.ApplicationException);
          return;
        }

        const responseObject = await res.json();

        setParent(new URLPath().getParent(locationSearch));

        var pageType = new CastToInterface().getPageType(responseObject);
        if (pageType === PageType.NotFound || pageType === PageType.ApplicationException) return;

        setPageType(pageType);
        switch (pageType) {
          case PageType.Archive:
            var archiveMedia = new CastToInterface().MediaArchive(responseObject);
            setArchive(archiveMedia.data);
            break;
          case PageType.DetailView:
            var detailViewMedia = new CastToInterface().MediaDetailView(responseObject);
            setDetailView(detailViewMedia.data);
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

  return {
    archive,
    detailView,
    pageType,
    parent
  };
};

export default useFileList;