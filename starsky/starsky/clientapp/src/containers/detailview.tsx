import React, { useEffect } from 'react';
import DetailViewSidebar from '../components/detail-view-sidebar';
import Preloader from '../components/preloader';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import useLocation from '../hooks/use-location';
import { IDetailView } from '../interfaces/IDetailView';
import { INavigateState } from '../interfaces/INavigateState';
import { Keyboard } from '../shared/keyboard';
import { Query } from '../shared/query';
import { URLPath } from '../shared/url-path';

const DetailView: React.FC<IDetailView> = (props) => {

  var history = useLocation();

  let fileIndexItem = props.fileIndexItem;
  let relativeObjects = props.relativeObjects;

  const [isDetails, setDetails] = React.useState(new URLPath().StringToIUrl(history.location.search).details);
  useEffect(() => {
    var details = new URLPath().StringToIUrl(history.location.search).details;
    setDetails(details);
  }, [history.location.search]);


  const [isTranslateRotation, setTranslateRotation] = React.useState(false);
  useEffect(() => {
    if (!props.fileIndexItem) return;
    (async () => {
      var thumbnailIsReady = await new Query().queryThumbnailApi(props.fileIndexItem.fileHash)
      setTranslateRotation(!thumbnailIsReady);
    })();
  }, [props.subPath]);

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
    if (new Keyboard().isInForm(event)) return;
    var parentDirectory = new URLPath().updateFilePath(history.location.search, fileIndexItem.parentDirectory);

    history.navigate(parentDirectory, {
      state: {
        fileName: fileIndexItem.fileName
      } as INavigateState
    });

  }, [fileIndexItem])

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
  }, [props.subPath]);

  // Reset Loading after changing page
  const [isLoading, setIsLoading] = React.useState(true);

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
    console.log(prev);

    setIsLoading(true)
    history.navigate(prev, { replace: true });
  }

  if (!fileIndexItem || !relativeObjects) {
    return (<Preloader parent={"/"} isDetailMenu={true} isOverlay={true}></Preloader>)
  }

  return (<div className={isDetails ? "detailview detailview--edit" : "detailview"}>
    {isLoading ? <Preloader parent={fileIndexItem.parentDirectory} isDetailMenu={true} isOverlay={true}></Preloader> : ""}
    {isDetails ? <DetailViewSidebar fileIndexItem={fileIndexItem} filePath={fileIndexItem.filePath}></DetailViewSidebar> : null}
    <div className={isError ? "main main--error" : "main main--" + fileIndexItem.imageFormat}>

      {!isError ? <img alt={fileIndexItem.tags} className={isTranslateRotation ? "image--default " + fileIndexItem.orientation : "image--default Horizontal"}
        onLoad={() => {
          setError(false)
          setIsLoading(false)
        }}
        onError={() => {
          setError(true)
          setIsLoading(false)
        }} src={"/api/thumbnail/" + fileIndexItem.fileHash + ".jpg?issingleitem=true"} /> : null}

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
