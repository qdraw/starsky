import React, { memo, useEffect } from 'react';
import DetailViewSidebar from '../components/detail-view-sidebar';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import useLocation from '../hooks/use-location';
import { IDetailView } from '../interfaces/IDetailView';
import { Keyboard } from '../shared/keyboard';
import { URLPath } from '../shared/url-path';

interface IDetailViewProps {
  imageFormat?: any;
  collections: boolean;
}

const DetailView: React.FunctionComponent<IDetailView> = memo((props) => {

  if (!props.fileIndexItem) return (<>no fileIndexItem in detailView</>);
  var history = useLocation();

  var fileIndexItem = props.fileIndexItem;
  var relativeObjects = props.relativeObjects;

  // Edit/Details screen
  const [isDetails, setDetails] = React.useState(new URLPath().StringToIUrl(history.location.search).details);
  useEffect(() => {
    var details = new URLPath().StringToIUrl(history.location.search).details;
    if (details) {
      setDetails(details);
    }
    else {
      setDetails(false);
    }
  }, [history.location.search]);


  // Reset Error after changing page
  const [isError, setError] = React.useState(false);
  useEffect(() => {
    setError(false);
  }, [props]);


  function go(path: string) {
    history.navigate(path);
  }

  function next() {
    var next = new URLPath().updateFilePath(history.location.search, relativeObjects.nextFilePath);
    go(next);
  }

  function prev() {
    var prev = new URLPath().updateFilePath(history.location.search, relativeObjects.prevFilePath);
    go(prev);
  }

  useKeyboardEvent(/Escape/, (event: KeyboardEvent) => {
    var parentDirectory = new URLPath().updateFilePath(history.location.search, fileIndexItem.parentDirectory);
    go(parentDirectory);
  })

  useKeyboardEvent(/ArrowLeft/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    if (!relativeObjects.prevFilePath) return;
    prev();
  })

  useKeyboardEvent(/ArrowRight/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    if (!relativeObjects.nextFilePath) return;
    next();
  })

  return (<div className={isDetails ? "detailview detailview--edit" : "detailview"}>

    {/* {{isLoading ? <Preloader parent={fileIndexItem.parentDirectory} isDetailMenu={true} isOverlay={true}></Preloader> : ""}} */}

    {isDetails ? <DetailViewSidebar fileIndexItem={fileIndexItem} filePath={fileIndexItem.filePath}></DetailViewSidebar> : ""}

    <div className={isError ? "main main--error" : "main main--" + fileIndexItem.imageFormat}>

      {isError ? "" : <img className={"image--default " + fileIndexItem.orientation} onLoad={() => setError(false)}
        onError={() => setError(true)} src={"/api/thumbnail/" + fileIndexItem.fileHash + ".jpg?issingleitem=True"} />}

      {relativeObjects.nextFilePath ?
        <div onClick={() => next()} className="nextprev nextprev--next"><div className="icon"></div></div>
        : ""}

      {relativeObjects.prevFilePath ?
        <div onClick={() => prev()}
          className="nextprev"><div className="icon"></div></div>
        : <div className="nextprev"></div>}

    </div>
  </div>)
});

export default DetailView;
