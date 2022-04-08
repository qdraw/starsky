import React, { useEffect } from "react";
import DetailView from "../containers/detailview";
import {
  DetailViewAction,
  DetailViewContextProvider,
  useDetailViewContext
} from "../contexts/detailview-context";
import { useSocketsEventName } from "../hooks/realtime/use-sockets.const";
import { IApiNotificationResponseModel } from "../interfaces/IApiNotificationResponseModel";
import { IDetailView } from "../interfaces/IDetailView";
import { IFileIndexItem } from "../interfaces/IFileIndexItem";
import DocumentTitle from "../shared/document-title";
import { FileListCache } from "../shared/filelist-cache";
import { URLPath } from "../shared/url-path";

/**
 * Used for search and list of files
 * @param detailview Detailview content
 */
export default function DetailViewContextWrapper(detailview: IDetailView) {
  return (
    <DetailViewContextProvider>
      <DetailViewWrapper {...detailview} />
    </DetailViewContextProvider>
  );
}

function DetailViewWrapper(detailViewProp: IDetailView) {
  let { state, dispatch } = useDetailViewContext();

  // Gets the content of the props and inject into the state
  useEffect(() => {
    if (!detailViewProp || !detailViewProp.fileIndexItem) return;
    dispatch({ type: "reset", payload: detailViewProp });
    // should run only at start or change page
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [detailViewProp.subPath]);

  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  DetailViewEventListenerUseEffect(dispatch);

  if (!state || !state.fileIndexItem) return null;

  return <DetailView {...state} />;
}

/**
 * Effect that run on startup of the component and updates the changes from other clients
 * @param dispatch - function to update the state
 */
export function DetailViewEventListenerUseEffect(
  dispatch: React.Dispatch<DetailViewAction>
) {
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
}

/**
 * Update DetailView from Event
 * @param event - CustomEvent with IFileIndexItem array
 * @param dispatch - function to update the state
 */
function updateDetailViewFromEvent(
  event: Event,
  dispatch: React.Dispatch<DetailViewAction>
) {
  const pushMessages = (
    event as CustomEvent<IApiNotificationResponseModel<IFileIndexItem[]>>
  ).detail;
  // useLocation, state or detailView is here always the default value
  var locationPath = new URLPath().StringToIUrl(window.location.search).f;

  for (const pushMessage of pushMessages.data) {
    // only update the state of the current view
    if (locationPath !== pushMessage.filePath) {
      // we choose to remove everything to avoid display errors
      new FileListCache().CacheCleanEverything();
      continue;
    }
    dispatch({
      type: "update",
      ...pushMessage,
      colorclass: pushMessage.colorClass
    });
  }
}
