import React, { useEffect } from 'react';
import DetailView from '../containers/detailview';
import { DetailViewContext, DetailViewContextProvider } from '../contexts/detailview-context';
import { IDetailView } from '../interfaces/IDetailView';
import DocumentTitle from '../shared/document-title';

/**
 * Used for search and list of files
 * @param archive the archive props 
 */
function DetailViewContextWrapper(detailview: IDetailView) {
  return (<DetailViewContextProvider>
    <DetailViewWrapper {...detailview} />
  </DetailViewContextProvider>)
};

function DetailViewWrapper(detailViewProp: IDetailView) {
  let { state, dispatch } = React.useContext(DetailViewContext);
  dispatch({ type: 'reset', payload: detailViewProp })

  // To update the list of items
  const [detailView, setDetailView] = React.useState(detailViewProp);
  useEffect(() => {
    if (!state.fileIndexItem) return;
    setDetailView(state);
  }, [state]);

  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  if (!state) return (<>(DetailViewWrapper) => no state</>)
  if (!state.fileIndexItem) return (<></>);
  console.log(state);

  return (
    <DetailView {...detailView} />
  )
};

export default DetailViewContextWrapper;
