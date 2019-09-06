import React, { useEffect } from 'react';
import DetailViewSidebar from '../components/detail-view-sidebar';
import MenuDetailView from '../components/menu-detailview';
import Preloader from '../components/preloader';
import { DetailViewContext } from '../contexts/detailview-context';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import useLocation from '../hooks/use-location';
import { IDetailView, newIRelativeObjects } from '../interfaces/IDetailView';
import { INavigateState } from '../interfaces/INavigateState';
import DocumentTitle from '../shared/document-title';
import { Keyboard } from '../shared/keyboard';
import { URLPath } from '../shared/url-path';
// const DetailView: React.FC<IDetailView> = () => {
//   return (<></>)
// };

const DetailView: React.FC<IDetailView> = () => {

  var history = useLocation();

  let { state, dispatch } = React.useContext(DetailViewContext);

  // var state: IDetailView | undefined = undefined;

  let relativeObjects = newIRelativeObjects();
  if (state && state.relativeObjects) {
    relativeObjects = state.relativeObjects;
  }

  const [isDetails, setDetails] = React.useState(new URLPath().StringToIUrl(history.location.search).details);
  useEffect(() => {
    var details = new URLPath().StringToIUrl(history.location.search).details;
    setDetails(details);
  }, [history.location.search]);

  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  const [isTranslateRotation, setTranslateRotation] = React.useState(false);
  // useEffect(() => {
  //   if (!state) return;
  //   if (!state.fileIndexItem) return;
  //   // Safari for iOS I don't need thumbnail rotation (for Mac it does need rotation)
  //   if (new BrowserDetect().IsIOS()) {
  //     return;
  //   };
  //   (async () => {
  //     var thumbnailIsReady = await new Query().queryThumbnailApi(state.fileIndexItem.fileHash)
  //     setTranslateRotation(!thumbnailIsReady);
  //   })();
  // }, [state.subPath]);

  useKeyboardEvent(/ArrowLeft/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    if (!relativeObjects) return;
    if (!relativeObjects.prevFilePath) return;
    prev();
  }, [relativeObjects])

  useKeyboardEvent(/ArrowRight/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    if (!relativeObjects) return;
    if (!relativeObjects.nextFilePath) return;
    next();
  }, [relativeObjects])

  useKeyboardEvent(/Escape/, (event: KeyboardEvent) => {
    if (!history.location) return;
    if (new Keyboard().isInForm(event)) return;
    var parentDirectory = new URLPath().updateFilePath(history.location.search, state.fileIndexItem.parentDirectory);

    history.navigate(parentDirectory, {
      state: {
        fileName: state.fileIndexItem.fileName
      } as INavigateState
    });
  }, [state.fileIndexItem])

  function toggleLabels() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !isDetails;
    setDetails(urlObject.details);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true })
  }

  useKeyboardEvent(/^(d)$/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    toggleLabels();
  }, [history.location.search])

  // Reset Error after changing page
  const [isError, setError] = React.useState(false);
  useEffect(() => {
    setError(false);
  }, [state.subPath]);

  // Reset Loading after changing page
  const [isLoading, setIsLoading] = React.useState(true);

  // // update states
  // const [status, setStatus] = React.useState(IExifStatus.Default);
  // useEffect(() => {
  //   if (!state.fileIndexItem || !state.fileIndexItem.status) return;
  //   setStatus(state.fileIndexItem.status);
  // }, [state]);


  // // update states
  // const [fileIndexItem, setFileIndexItem] = React.useState(newIFileIndexItem());
  // useEffect(() => {
  //   if (!state.fileIndexItem || !state.fileIndexItem) return;
  //   setFileIndexItem(state.fileIndexItem);
  // }, [state.fileIndexItem]);


  function next() {
    if (!relativeObjects) return;
    var next = updateUrl(relativeObjects.nextFilePath);
    setIsLoading(true)
    history.navigate(next, { replace: true });
  }

  function updateUrl(toUpdateFilePath: string) {
    var url = new URLPath().StringToIUrl(history.location.search);
    url.f = toUpdateFilePath;
    url.details = isDetails;
    return "/beta" + new URLPath().IUrlToString(url);
  }

  function prev() {
    if (!relativeObjects) return;
    var prev = updateUrl(relativeObjects.prevFilePath);
    setIsLoading(true)
    history.navigate(prev, { replace: true });
  }

  if (!state.fileIndexItem || !relativeObjects) {
    return (<Preloader parent={"/"} isDetailMenu={true} isOverlay={true}></Preloader>)
  }

  return (<>
    <MenuDetailView />
    <div className={isDetails ? "detailview detailview--edit" : "detailview"}>
      {isLoading ? <Preloader parent={state.fileIndexItem.parentDirectory} isDetailMenu={true} isOverlay={true}></Preloader> : ""}

      {isDetails ? <DetailViewSidebar status={state.fileIndexItem.status}
        fileIndexItem={state.fileIndexItem} filePath={state.fileIndexItem.filePath}></DetailViewSidebar> : null}

      <div className={isError ? "main main--error" : "main main--" + state.fileIndexItem.imageFormat}>

        {!isError ? <img alt={state.fileIndexItem.tags}
          className={isTranslateRotation ? "image--default " + state.fileIndexItem.orientation : "image--default Horizontal"}
          onLoad={() => {
            setError(false)
            setIsLoading(false)
          }}
          onError={() => {
            setError(true)
            setIsLoading(false)
          }} src={"/api/thumbnail/" + state.fileIndexItem.fileHash + ".jpg?issingleitem=true"} /> : null}

        {relativeObjects.nextFilePath ?
          <div onClick={() => next()} className="nextprev nextprev--next"><div className="icon"></div></div>
          : ""}

        {relativeObjects.prevFilePath ?
          <div onClick={() => prev()}
            className="nextprev"><div className="icon"></div></div>
          : <div className="nextprev"></div>}

      </div>
    </div>
  </>)
};

export default DetailView;
