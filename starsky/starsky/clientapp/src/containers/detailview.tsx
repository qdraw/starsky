import React, { useEffect } from 'react';
import DetailViewSidebar from '../components/detail-view-sidebar';
import MenuDetailView from '../components/menu-detailview';
import Preloader from '../components/preloader';
import { DetailViewContext } from '../contexts/detailview-context';
import useFetch from '../hooks/use-fetch';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import useLocation from '../hooks/use-location';
import { IDetailView, newIRelativeObjects } from '../interfaces/IDetailView';
import { Orientation } from '../interfaces/IFileIndexItem';
import { INavigateState } from '../interfaces/INavigateState';
import BrowserDetect from '../shared/browser-detect';
import DocumentTitle from '../shared/document-title';
import { Keyboard } from '../shared/keyboard';
import { Query } from '../shared/query';
import { URLPath } from '../shared/url-path';

const DetailView: React.FC<IDetailView> = () => {

  var history = useLocation();

  let { state } = React.useContext(DetailViewContext);

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
  }, [history.location.search]);

  // To Get the rotation update
  const [translateRotation, setTranslateRotation] = React.useState(Orientation.Horizontal);
  var location = new Query().UrlQueryThumbnailApi(state.fileIndexItem.fileHash);
  const responseObject = useFetch(location, 'get');
  useEffect(() => {
    if (!responseObject) return;
    if (!state.fileIndexItem.orientation) return;
    // Safari for iOS I don't need thumbnail rotation (for Mac it require rotation)
    if (new BrowserDetect().IsIOS()) {
      return;
    }
    var statusCode: number = responseObject.statusCode;
    if (statusCode === 200) {
      setTranslateRotation(Orientation.Horizontal);
    }
    else if (statusCode === 202) {
      setTranslateRotation(state.fileIndexItem.orientation);
      return;
    }
  }, [responseObject]);
  console.log(translateRotation);

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

  function next() {
    if (!relativeObjects) return;
    var next = updateUrl(relativeObjects.nextFilePath);
    // Keeps loading forever
    if (relativeObjects.nextHash !== state.fileIndexItem.fileHash) {
      setIsLoading(true)
    }
    history.navigate(next, { replace: true });
  }

  function updateUrl(toUpdateFilePath: string) {
    var url = new URLPath().StringToIUrl(history.location.search);
    url.f = toUpdateFilePath;
    url.details = isDetails;
    return "/" + new URLPath().IUrlToString(url);
  }

  function prev() {
    if (!relativeObjects) return;
    var prev = updateUrl(relativeObjects.prevFilePath);
    // Keeps loading forever
    if (relativeObjects.prevHash !== state.fileIndexItem.fileHash) {
      setIsLoading(true)
    }
    history.navigate(prev, { replace: true });
  }

  if (!state.fileIndexItem || !relativeObjects) {
    return (<Preloader parent={"/"} isDetailMenu={true} isOverlay={true}></Preloader>)
  }

  return (<>
    <MenuDetailView />
    <div className={isDetails ? "detailview detailview--edit" : "detailview"}>
      {isLoading ? <Preloader parent={state.fileIndexItem.parentDirectory} isDetailMenu={true} isOverlay={true}></Preloader> : ""}

      {isDetails ? <DetailViewSidebar status={state.fileIndexItem.status} filePath={state.fileIndexItem.filePath}></DetailViewSidebar> : null}

      <div className={isError ? "main main--error" : "main main--" + state.fileIndexItem.imageFormat}>

        {!isError && state.fileIndexItem.fileHash ? <img alt={state.fileIndexItem.tags}
          className={"image--default " + translateRotation}
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
            className="nextprev nextprev--prev"><div className="icon"></div></div>
          : <div className="nextprev nextprev"></div>}

      </div>
    </div>
  </>)
};

export default DetailView;
