import React, { useEffect } from "react";
import Preloader from "../components/atoms/preloader/preloader";
import Archive from "../containers/archive/archive";
import { Login } from "../containers/login";
import Search from "../containers/search";
import Trash from "../containers/trash";
import { ArchiveAction, ArchiveContext, ArchiveContextProvider } from "../contexts/archive-context";
import { useSocketsEventName } from "../hooks/realtime/use-sockets.const";
import { IApiNotificationResponseModel } from "../interfaces/IApiNotificationResponseModel";
import { IArchiveProps } from "../interfaces/IArchiveProps";
import { PageType } from "../interfaces/IDetailView";
import { IExifStatus } from "../interfaces/IExifStatus";
import { IFileIndexItem } from "../interfaces/IFileIndexItem";
import { DocumentTitle } from "../shared/document-title";
import { FileListCache } from "../shared/filelist-cache";
import { URLPath } from "../shared/url/url-path";

/**
 * Used for search and list of files
 * @param archive the archive props
 */
export default function ArchiveContextWrapper(archive: Readonly<IArchiveProps>) {
  return (
    <ArchiveContextProvider>
      <ArchiveWrapper {...archive} />
    </ArchiveContextProvider>
  );
}

function ArchiveWrapper(archive: Readonly<IArchiveProps>) {
  const { state, dispatch } = React.useContext(ArchiveContext);

  /**
   * Running on changing searchQuery or subpath
   */
  useEffect(() => {
    // don't update the cache
    dispatch({ type: "set", payload: archive });
    // disable to prevent duplicate api calls
    // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
  }, [archive.subPath, archive.searchQuery, archive.pageNumber, archive.colorClassUsage]);

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
export function ArchiveEventListenerUseEffect(dispatch: React.Dispatch<ArchiveAction>) {
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
function updateArchiveFromEvent(event: Event, dispatch: React.Dispatch<ArchiveAction>) {
  const pushMessagesEvent = (event as CustomEvent<IApiNotificationResponseModel<IFileIndexItem[]>>)
    .detail;
  // useLocation, state or archive is here always the default value
  const parentLocationPath = new URLPath().StringToIUrl(window.location.search).f;

  dispatchEmptyFolder(pushMessagesEvent.data, parentLocationPath, dispatch);

  const toAddedFiles = filterArchiveFromEvent(pushMessagesEvent.data, parentLocationPath);

  dispatch({ type: "add", add: toAddedFiles });
}

/**
 * When a folder is renamed there is item send with status
 * @param itemList - list of items that contains
 * @param parentLocationPath - the path to check
 * @param dispatch - send reset
 * @returns
 */
export function dispatchEmptyFolder(
  itemList: IFileIndexItem[],
  parentLocationPath: string | undefined,
  dispatch: (value: ArchiveAction) => void
) {
  const parentItems = itemList.filter((p) => p.filePath === parentLocationPath);
  if (parentItems.length === 1 && parentItems[0].status === IExifStatus.NotFoundSourceMissing) {
    dispatch({
      type: "remove-folder"
    });
  }
}

/**
 * Filter items that are not in the current folder
 * @param pushMessagesEvent - list of items
 * @param parentLocationPath - path of the folder
 * @returns filtered items
 */
export function filterArchiveFromEvent(
  pushMessagesEvent: IFileIndexItem[],
  parentLocationPath?: string
) {
  parentLocationPath ??= "/";

  const toAddedFiles = [];
  for (const pushMessage of pushMessagesEvent) {
    // only update in current directory view && parent directory
    if (parentLocationPath !== pushMessage.parentDirectory) {
      // we choose to remove everything to avoid display errors
      new FileListCache().CacheCleanEverything();
      continue;
    }
    toAddedFiles.push(pushMessage);
  }
  return toAddedFiles;
}
