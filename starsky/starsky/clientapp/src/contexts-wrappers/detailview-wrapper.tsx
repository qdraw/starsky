import React, { useEffect } from 'react';
import DetailView from '../containers/detailview';
import { DetailViewContextProvider, useDetailViewContext } from '../contexts/detailview-context';
import { useSocketsEventName } from '../hooks/use-sockets';
import { IDetailView } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import DocumentTitle from '../shared/document-title';
import { URLPath } from '../shared/url-path';

/**
 * Used for search and list of files
 * @param detailview Detailview content
 */
function DetailViewContextWrapper(detailview: IDetailView) {
  return (<DetailViewContextProvider>
    <DetailViewWrapper {...detailview} />
  </DetailViewContextProvider>)
}

function DetailViewWrapper(detailViewProp: IDetailView) {
  let { state, dispatch } = useDetailViewContext();

  useEffect(() => {
    if (!detailViewProp || !detailViewProp.fileIndexItem) return;
    dispatch({ type: 'reset', payload: detailViewProp });
    // should run only at start or change page
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [detailViewProp.subPath]);

  // To update the list of items
  const [detailView, setDetailView] = React.useState(detailViewProp);
  useEffect(() => {
    if (!state) return;
    if (!state.fileIndexItem) return;
    setDetailView(state);
  }, [state]);

  function updateDetailView(event: Event) {
    if (!detailView || !detailView.fileIndexItem) return;
    const pushMessage = event as CustomEvent<IFileIndexItem>;

    // useLocation, state or detailView is here always the default value
    var locationPath = new URLPath().StringToIUrl(window.location.search).f
    if (locationPath !== pushMessage.detail.filePath) {
      return;
    }
    dispatch({ type: 'update', ...pushMessage.detail, colorclass: pushMessage.detail.colorClass });
  }

  useEffect(() => {
    document.body.addEventListener(useSocketsEventName, updateDetailView);
    return () => {
      document.body.removeEventListener(useSocketsEventName, updateDetailView);
    };
    // only when start of view
    // eslint-disable-next-line
  }, []);


  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  if (!state) return (<>(DetailViewWrapper) = no state</>)
  if (!state.fileIndexItem) return (<></>);

  return (<DetailView {...state} />)
}

export default DetailViewContextWrapper;
