import React, { useEffect } from 'react';
import DetailView from '../containers/detailview';
import { DetailViewContextProvider, useDetailViewContext } from '../contexts/detailview-context';
import { IDetailView } from '../interfaces/IDetailView';
import DocumentTitle from '../shared/document-title';

/**
 * Used for search and list of files
 * @param detailview Detailview content
 */
function DetailViewContextWrapper(detailview: IDetailView) {
  return (<DetailViewContextProvider>
    <DetailViewWrapper {...detailview} />
  </DetailViewContextProvider>)
}

function DetailViewWrapper(detailViewProp: IDetailView) {
  let { state, dispatch } = useDetailViewContext();

  // Gets the content of the props and inject into the state
  useEffect(() => {
    if (!detailViewProp || !detailViewProp.fileIndexItem) return;
    dispatch({ type: 'reset', payload: detailViewProp });
    // should run only at start or change page
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [detailViewProp.subPath]);

  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  if (!state) return (<>(DetailViewWrapper) = no state</>)
  if (!state.fileIndexItem) return (<></>);

  return (<DetailView {...state} />)
}

export default DetailViewContextWrapper;
