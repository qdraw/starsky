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

  if (!state) return (<>(DetailViewWrapper) = no state</>)
  if (!state.fileIndexItem) return (<></>);

  return (<DetailView {...state} />)
}

function updateDetailViewFromEvent(event: Event, dispatch: React.Dispatch<DetailViewAction>) {
  const pushMessages = (event as CustomEvent<IFileIndexItem[]>).detail;
  var locationPath = new URLPath().StringToIUrl(window.location.search).f

  for (let index = 0; index < pushMessages.length; index++) {
    const pushMessage = pushMessages[index];
    // useLocation, state or detailView is here always the default value
    if (locationPath !== pushMessage.filePath) {
      continue;
    }
    dispatch({ type: 'update', ...pushMessage, colorclass: pushMessage.colorClass });
  }

}