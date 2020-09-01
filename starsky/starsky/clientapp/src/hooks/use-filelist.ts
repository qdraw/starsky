import { useEffect, useState } from 'react';
import { IArchive, newIArchive } from '../interfaces/IArchive';
import { IDetailView, newDetailView, PageType } from '../interfaces/IDetailView';
import { CastToInterface } from '../shared/cast-to-interface';
import { DifferenceInDate } from '../shared/date';
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
  var location = new UrlQuery().UrlQueryServerApi(locationSearch);
  const cacheName = `starsky`;

  // Get data from the cache.
  async function getCachedData(cacheLocalName: string, url: string): Promise<any | false> {

    // not supported in your browser
    if (!('caches' in window)) {
      return false;
    }

    let cacheStorage: Cache;
    try {
      // not allowed witin the http context
      cacheStorage = await caches.open(cacheLocalName)
    } catch (error) {
      return false;
    }

    const cachedResponse = await cacheStorage.match(url);

    if (!cachedResponse || !cachedResponse.ok || !checkIfOld(cachedResponse)) {
      return false;
    }
    return await cachedResponse.json();
  }

  const checkIfOld = (cachedResponse: Response) => {
    var date = cachedResponse.headers.get('date');
    if (!date) return false;
    var diff = DifferenceInDate(new Date(date).valueOf());
    var diffResult = diff < 2; // 2 minutes
    return diffResult;
  }

  const fetchContent = async (location: string, abortController: AbortController): Promise<void> => {

    try {
      // force start with a loading icon 
      if (resetPageTypeBeforeLoading) setPageType(PageType.Loading);

      let cachedData = await getCachedData(cacheName, location);
      if (cachedData) {
        console.log('Retrieved cached data', cachedData);
        setPageTypeHelper(cachedData);
        return;
      }

      let response: Response = await fetch(location, {
        signal: abortController.signal,
        credentials: "include",
        method: 'get'
      });

      if (response.status === 404) {
        setPageType(PageType.NotFound);
        return;
      }
      else if (response.status === 401) {
        setPageType(PageType.Unauthorized);
        return;
      }
      else if (response.status >= 400 && response.status <= 550) {
        setPageType(PageType.ApplicationException);
        return;
      }

      const responseObject = await response.json();
      setPageTypeHelper(responseObject);

      // add it to the cache
      const cacheStorage = await caches.open(cacheName);
      await cacheStorage.put(location, new Response(JSON.stringify(responseObject), response));

    } catch (e) {
      console.error(e);
    }
  }

  const setPageTypeHelper = (responseObject: any) => {
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
  }

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
    detailView,
    pageType,
    parent,
    fetchContent,
  };
};

export default useFileList;