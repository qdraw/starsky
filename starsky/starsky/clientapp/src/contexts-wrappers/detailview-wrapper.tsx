import React, { useEffect } from 'react';
import DetailView from '../containers/detailview';
import { DetailViewAction, DetailViewContextProvider, useDetailViewContext } from '../contexts/detailview-context';
import { useSocketsEventName } from '../hooks/realtime/use-sockets.const';
import { IDetailView } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import DocumentTitle from '../shared/document-title';
import { URLPath } from '../shared/url-path';

/**
 * Used for search and list of files
 * @param detailview Detailview content
 */
export default function DetailViewContextWrapper(detailview: IDetailView) {
  return (<DetailViewContextProvider>
    <DetailViewWrapper {...detailview} />
  </DetailViewContextProvider>)
}

function DetailViewWrapper(detailViewProp: IDetailView) {
  let { state, dispatch } = useDetailViewContext();

  // Gets the content of the props and inject into the state
  useEffect(() => {
    if (!detailViewProp || !detailViewProp.fileIndexItem) return;
    dispatch({ type: 'reset', payload: detailViewProp });
    // should run only at start or change page
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [detailViewProp.subPath]);

  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  if (!state) return (<>(DetailViewWrapper) = no state</>)
  if (!state.fileIndexItem) return (<></>);

  // Catch events from updates
  const update = (event: Event) => updateDetailViewFromEvent(event, dispatch);
  useEffect(() => {
    document.body.addEventListener(useSocketsEventName, update);
    return () => {
      document.body.removeEventListener(useSocketsEventName, update);
    };
    // only when start of view
    // eslint-disable-next-line
  }, []);

  return (<DetailView {...state} />)
}

function updateDetailViewFromEvent(event: Event, dispatch: React.Dispatch<DetailViewAction>) {
  const pushMessage = event as CustomEvent<IFileIndexItem>;

  // useLocation, state or detailView is here always the default value
  var locationPath = new URLPath().StringToIUrl(window.location.search).f
  if (locationPath !== pushMessage.detail.filePath) {
    return;
  }
  dispatch({ type: 'update', ...pushMessage.detail, colorclass: pushMessage.detail.colorClass });
}