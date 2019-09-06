import React, { useEffect } from 'react';
import Archive from '../containers/archive';
import Search from '../containers/search';
import { ArchiveContext, ArchiveContextProvider } from '../contexts/archive-context';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import DocumentTitle from '../shared/document-title';

/**
 * Used for search and list of files
 * @param archive the archive props 
 */
function ArchiveContextWrapper(archive: IArchiveProps) {
  return (<ArchiveContextProvider>
    <ArchiveWrapper {...archive} />
  </ArchiveContextProvider>)
};

function ArchiveWrapper(archive: IArchiveProps) {
  let { state, dispatch } = React.useContext(ArchiveContext);
  dispatch({ type: 'reset', payload: archive })

  // To update the list of items
  const [archiveList, setArchiveList] = React.useState(archive);
  useEffect(() => {
    if (!state.fileIndexItems) return;
    setArchiveList(state);
  }, [state]);

  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  if (!state) return (<>(ArchiveWrapper) => no state</>)
  if (!state.fileIndexItems) return (<></>);
  if (!state.pageType) return (<></>);

  if (state.pageType === "Search") {
    return (
      <Search {...archiveList} />
    )
  }
  return (
    <Archive {...archiveList} />
  )
};

export default ArchiveContextWrapper;
