import { useContext, useEffect, useState } from 'react';
import HistoryContext from '../contexts/history-contexts';
import { newIArchive } from '../interfaces/IArchive';
import { newDetailView, PageType } from '../interfaces/IDetailView';
import { CastToInterface } from '../shared/cast-to-interface';
import { Query } from '../shared/query';
import { URLPath } from '../shared/url-path';
import useFetch from './use-fetch';

const useFileList = (): any | null => {
  const history = useContext(HistoryContext);
  const [archive, setArchive] = useState(newIArchive());
  const [detailView, setDetailView] = useState(newDetailView());
  const [pageType, setPageType] = useState(PageType.Loading);
  const [parent, setParent] = useState('/');

  var location = new Query().UrlQueryServerApi(history.location.search);
  var responseObject = useFetch(location, 'get');

  useEffect(() => {
    if (!responseObject) return;
    setParent(new URLPath().getParent(history.location.search));
    var pageType = new CastToInterface().getPageType(responseObject);
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
  }, [responseObject]);
  return {
    archive,
    detailView,
    pageType,
    parent
  };
};
export default useFileList;
