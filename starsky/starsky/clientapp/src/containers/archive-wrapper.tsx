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
    if (state.fileIndexItems.length === 0) return;

    // state.fileIndexItems.forEach((element, index) => {
    //   state.fileIndexItems.splice(index, 1)
    //   state.fileIndexItems.push(element);
    // });

    // state.fileIndexItems.sort(function (a, b) {
    //   var x = a.fileName.toLowerCase();
    //   var y = b.fileName.toLowerCase();
    //   return x < y ? -1 : x > y ? 1 : 0;
    // });

    setArchiveList(state);
  }, [state]);

  if (!state.fileIndexItems) return (<></>);

  return (
    <Archive {...archiveList} />
  )
};

export default ArchiveContextWrapper;
