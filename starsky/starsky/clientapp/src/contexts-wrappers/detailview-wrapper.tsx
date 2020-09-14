import React, { useEffect } from 'react';
import DetailView from '../containers/detailview';
import { DetailViewContext, DetailViewContextProvider } from '../contexts/detailview-context';
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
  let { state, dispatch } = React.useContext(DetailViewContext);
  dispatch({ type: 'reset', payload: detailViewProp });

  // To update the list of items
  const [detailView, setDetailView] = React.useState(detailViewProp);
  useEffect(() => {
    if (!state) return;
    if (!state.fileIndexItem) return;
    setDetailView(state);
  }, [state]);


  // useEffect(() => {
  //   function updateDetailView(event: Event) {
  //     const pushMessage = event as CustomEvent<IFileIndexItem>;
  //     dispatch({ type: 'reset', payload: { ...state, fileIndexItem: pushMessage.detail } });
  //   }

  //   document.body.addEventListener(useSocketsEventName, updateDetailView);
  //   return () => {
  //     document.body.removeEventListener(useSocketsEventName, updateDetailView);
  //   };
  //   // only when start of view
  //   // eslint-disable-next-line
  // }, []);


  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  if (!state) return (<>(DetailViewWrapper) = no state</>)
  if (!state.fileIndexItem) return (<></>);

  return (
    <>
      <DetailView {...detailView} />
    </>
  )
}

export default DetailViewContextWrapper;
