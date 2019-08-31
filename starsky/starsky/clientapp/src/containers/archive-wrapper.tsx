import React, { useEffect } from 'react';
import { ArchiveContext, ArchiveContextProvider } from '../contexts/archive-context';
import { IArchiveProps } from '../interfaces/IArchiveProps';
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
    setArchiveList(state);
  }, [state]);

  if (!state.fileIndexItems) return (<></>);

  return (
    <Archive {...archiveList} />
  )
};

export default ArchiveContextWrapper;
