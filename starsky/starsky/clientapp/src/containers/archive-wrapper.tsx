import React, { useEffect } from 'react';
import { ArchiveContext, ArchiveContextProvider } from '../contexts/archive-context';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { newIFileIndexItemArray } from '../interfaces/IFileIndexItem';
import Archive from './archive';

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
    var fileIndexItems = newIFileIndexItemArray();
    state.fileIndexItems.forEach(item => {
      fileIndexItems.push(item)
    });
    var updatedState = state;
    updatedState.fileIndexItems = fileIndexItems;
    setArchiveList(updatedState);
  }, [state]);

  if (!state.fileIndexItems) return (<></>);

  return (
    <Archive {...archiveList} />
  )
};

export default ArchiveContextWrapper;
