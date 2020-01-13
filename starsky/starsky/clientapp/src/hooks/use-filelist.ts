import { useEffect, useState } from 'react';
import { IArchive, newIArchive } from '../interfaces/IArchive';
import { IDetailView, newDetailView, PageType } from '../interfaces/IDetailView';
import { CastToInterface } from '../shared/cast-to-interface';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';

export interface IFileList {
  archive?: IArchive,
  detailView?: IDetailView,
  pageType: PageType,
  parent: string,
  fetchContent: (location: string, abortController: AbortController) => Promise<void>;
}

/**
 * Hook to get index API
 * @param locationSearch with query parameter "?f=/"
 * @param resetPageTypeBeforeLoading start direct with loading state = true is enable, use false to have smooth page transistions
 */
const useFileList = (locationSearch: string, resetPageTypeBeforeLoading: boolean): IFileList | null => {

  const [archive, setArchive] = useState(newIArchive());
  const [detailView, setDetailView] = useState(newDetailView());
  const [pageType, setPageType] = useState(PageType.Loading);
  const [parent, setParent] = useState('/');

  const fetchContent = async (location: string, abortController: AbortController): Promise<void> => {
    try {

      // force start with a loading icon 
      if (resetPageTypeBeforeLoading) setPageType(PageType.Loading);

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

      if (!responseObject || !responseObject.pageType
        || responseObject.pageType === PageType.NotFound
        || responseObject.pageType === PageType.ApplicationException) return;

      setPageType(responseObject.pageType);
      switch (responseObject.pageType) {
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
  }

  useEffect(() => {
    const abortController = new AbortController();
    fetchContent(new UrlQuery().UrlQueryServerApi(locationSearch), abortController);

    return () => {
      abortController.abort();
    };

    // dependency: 'locationSearch'. is not added to avoid a lot of queries
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location]);

  return {
    archive,
    detailView,
    pageType,
    parent,
    fetchContent,
  };
};

export default useFileList;