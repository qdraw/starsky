import React, { useEffect } from 'react';
import Preloader from '../components/atoms/preloader/preloader';
import Archive from '../containers/archive';
import Login from '../containers/login';
import Search from '../containers/search';
import Trash from '../containers/trash';
import { ArchiveContext, ArchiveContextProvider } from '../contexts/archive-context';
import { useSocketsEventName } from '../hooks/use-sockets';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import DocumentTitle from '../shared/document-title';

/**
 * Used for search and list of files
 * @param archive the archive props 
 */
function ArchiveContextWrapper(archive: IArchiveProps) {
  return (<ArchiveContextProvider>
    <ArchiveWrapper {...archive} />
  </ArchiveContextProvider>)
}

function ArchiveWrapper(archive: IArchiveProps) {
  let { state, dispatch } = React.useContext(ArchiveContext);

  /**
   * Running on changing searchQuery or subpath
   */
  useEffect(() => {
    dispatch({ type: 'force-reset', payload: archive })
    // disable to prevent duplicate api calls
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [archive.subPath, archive.searchQuery, archive.pageNumber, archive.colorClassUsage]);


  useEffect(() => {
    function updateDetailView(event: Event) {
      const pushMessage = event as CustomEvent<IFileIndexItem>;
      dispatch({
        type: 'update', select: [pushMessage.detail.fileName],
        ...pushMessage.detail,
        colorclass: pushMessage.detail.colorClass
      });
    }

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

  if (!state) return (<>(ArchiveWrapper) = no state</>)
  if (!state.fileIndexItems) return (<></>);
  if (!state.pageType) return (<></>);

  switch (state.pageType) {
    case PageType.Trash:
      return (
        <Trash {...state} />
      );
    case PageType.Search:
      return (
        <Search {...state} />
      );
    case PageType.Unauthorized:
      return (
        <Login />
      );
    case PageType.Archive:
      return (
        <Archive {...state} />
      );
    default:
      return (
        <Preloader isOverlay={true} isTransition={false} />
      );
  }

}

export default ArchiveContextWrapper;
