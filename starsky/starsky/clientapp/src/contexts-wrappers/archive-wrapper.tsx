import React, { useEffect } from "react";
import Preloader from "../components/atoms/preloader/preloader";
import Archive from "../containers/archive";
import Login from "../containers/login";
import Search from "../containers/search";
import Trash from "../containers/trash";
import {
  ArchiveAction,
  ArchiveContext,
  ArchiveContextProvider
} from "../contexts/archive-context";
import { useSocketsEventName } from "../hooks/realtime/use-sockets.const";
import { IArchiveProps } from "../interfaces/IArchiveProps";
import { PageType } from "../interfaces/IDetailView";
import { IFileIndexItem } from "../interfaces/IFileIndexItem";
import DocumentTitle from "../shared/document-title";
import { FileListCache } from "../shared/filelist-cache";
import { URLPath } from "../shared/url-path";

/**
 * Used for search and list of files
 * @param archive the archive props
 */
export default function ArchiveContextWrapper(archive: IArchiveProps) {
  return (
    <ArchiveContextProvider>
      <ArchiveWrapper {...archive} />
    </ArchiveContextProvider>
  );
}

function ArchiveWrapper(archive: IArchiveProps) {
  let { state, dispatch } = React.useContext(ArchiveContext);

  /**
   * Running on changing searchQuery or subpath
   */
  useEffect(() => {
    // don't update the cache
    dispatch({ type: "set", payload: archive });
    // disable to prevent duplicate api calls
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    archive.subPath,
    archive.searchQuery,
    archive.pageNumber,
    archive.colorClassUsage
  ]);

  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  ArchiveEventListenerUseEffect(dispatch);

  if (!state) return <>(ArchiveWrapper) = no state</>;
  if (!state.fileIndexItems) return <></>;
  if (!state.pageType) return <></>;

  switch (state.pageType) {
    case PageType.Trash:
      return <Trash {...state} />;
    case PageType.Search:
      return <Search {...state} />;
    case PageType.Unauthorized:
      return <Login />;
    case PageType.Archive:
      return <Archive {...state} />;
    default:
      return <Preloader isOverlay={true} isTransition={false} />;
  }
}

/**
 * Effect that run on startup of the component and updates the changes from other clients
 * @param dispatch - function to update the state
 */
export function ArchiveEventListenerUseEffect(
  dispatch: React.Dispatch<ArchiveAction>
) {
  // Catch events from updates
  const update = (event: Event) => updateArchiveFromEvent(event, dispatch);
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
 * Update Archive from Event
 * @param event - CustomEvent with IFileIndexItem array
 * @param dispatch - function to update the state
 */
function updateArchiveFromEvent(
  event: Event,
  dispatch: React.Dispatch<ArchiveAction>
) {
  const pushMessagesEvent = (event as CustomEvent<IFileIndexItem[]>).detail;
  // useLocation, state or archive is here always the default value
  var parentLocationPath = new URLPath().StringToIUrl(window.location.search).f;

  var toAddedFiles = [];
  for (let index = 0; index < pushMessagesEvent.length; index++) {
    const pushMessage = pushMessagesEvent[index];
    // only update the state of the current view
    if (parentLocationPath !== pushMessage.parentDirectory) {
      // we choose to remove everything to avoid display errors
      new FileListCache().CacheCleanEverything();
      continue;
    }
    toAddedFiles.push(pushMessage);
  }
  dispatch({ type: "add", add: toAddedFiles });
}
