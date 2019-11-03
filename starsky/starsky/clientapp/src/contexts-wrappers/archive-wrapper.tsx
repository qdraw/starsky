import React, { useEffect } from 'react';
import Archive from '../containers/archive';
import Search from '../containers/search';
import Trash from '../containers/trash';
import { ArchiveContext, ArchiveContextProvider } from '../contexts/archive-context';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { PageType } from '../interfaces/IDetailView';
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

  // running on load
  useEffect(() => {
    if (archive.fileIndexItems) {
      console.log('running dispatch');
      dispatch({ type: 'reset', payload: archive })
    }
  }, [archive]);

  var archiveList = state;

  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  if (!state) return (<>(ArchiveWrapper) => no state</>)
  if (!state.fileIndexItems) return (<></>);
  if (!state.pageType) return (<></>);

  if (state.pageType === PageType.Trash) {
    return (
      <Trash {...archiveList} />
    )
  }

  if (state.pageType === PageType.Search) {
    return (
      <Search {...archiveList} />
    )
  }

  return (
    <Archive {...archiveList} />
  )
}

export default ArchiveContextWrapper;
