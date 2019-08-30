import React, { useEffect } from 'react';
import DetailViewSidebar from '../components/detail-view-sidebar';
import Preloader from '../components/preloader';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import useLocation from '../hooks/use-location';
import { IDetailView } from '../interfaces/IDetailView';
import { Keyboard } from '../shared/keyboard';
import { URLPath } from '../shared/url-path';

const DetailView: React.FC<IDetailView> = (props) => {

  var history = useLocation();

  let fileIndexItem = props.fileIndexItem;
  let relativeObjects = props.relativeObjects;

  // Edit/Details screen
  const [isDetails, setDetails] = React.useState(new URLPath().StringToIUrl(history.location.search).details);
  useEffect(() => {
    var details = new URLPath().StringToIUrl(history.location.search).details;
    setDetails(details);
  }, [history.location.search]);

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
    var parentDirectory = new URLPath().updateFilePath(history.location.search, fileIndexItem.parentDirectory);
    history.navigate(parentDirectory, {});
  }, [fileIndexItem])

  function toggleLabels() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !urlObject.details;
    setDetails(urlObject.details);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true })
  }

  useKeyboardEvent(/(i|e)/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    console.log('t');

    toggleLabels();
  }, [history.location.search])

  // Reset Error after changing page
  const [isError, setError] = React.useState(false);
  useEffect(() => {
    setError(false);
  }, [props]);

  // Reset Loading after changing page
  const [isLoading, setIsLoading] = React.useState(true);

  function next() {
    if (!relativeObjects) return;
    var next = new URLPath().updateFilePath(history.location.search, relativeObjects.nextFilePath);
    setIsLoading(true)
    history.navigate(next, {});
  }

  function prev() {
    if (!relativeObjects) return;
    var prev = new URLPath().updateFilePath(history.location.search, relativeObjects.prevFilePath);
    setIsLoading(true)
    history.navigate(prev, {});
  }

  if (!fileIndexItem || !relativeObjects) {
    return (<Preloader parent={"/"} isDetailMenu={true} isOverlay={true}></Preloader>)
  }

  return (<div className={isDetails ? "detailview detailview--edit" : "detailview"}>
    {isLoading ? <Preloader parent={fileIndexItem.parentDirectory} isDetailMenu={true} isOverlay={true}></Preloader> : ""}

    {isDetails ? <DetailViewSidebar fileIndexItem={fileIndexItem} filePath={fileIndexItem.filePath}></DetailViewSidebar> : ""}

    <div className={isError ? "main main--error" : "main main--" + fileIndexItem.imageFormat}>

      {isError ? "" : <img className={"image--default " + fileIndexItem.orientation}
        onLoad={() => {
          setError(false)
          setIsLoading(false)
        }}
        onError={() => {
          setError(true)
          setIsLoading(false)
        }} src={"/api/thumbnail/" + fileIndexItem.fileHash + ".jpg?issingleitem=True"} />}

      {relativeObjects.nextFilePath ?
        <div onClick={() => next()} className="nextprev nextprev--next"><div className="icon"></div></div>
        : ""}

      {relativeObjects.prevFilePath ?
        <div onClick={() => prev()}
          className="nextprev"><div className="icon"></div></div>
        : <div className="nextprev"></div>}

    </div>
  </div>)
};

export default DetailView;
