import React, { memo, useContext } from 'react';
import DetailViewSidebar from '../components/detail-view-sidebar';
import HistoryContext from '../contexts/history-contexts';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import { IDetailView } from '../interfaces/IDetailView';
import { Keyboard } from '../shared/keyboard';
import { URLPath } from '../shared/url-path';

interface IDetailViewProps {
  imageFormat?: any;
  collections: boolean;
}

const DetailView: React.FunctionComponent<IDetailView> = memo((props) => {

  if (!props.fileIndexItem) return (<>no fileIndexItem in detailView</>);
  const history = useContext(HistoryContext);

  var fileIndexItem = props.fileIndexItem;
  var relativeObjects = props.relativeObjects;

  // const [isEditMode, setEditMode] = React.useState(false);
  let isEditMode = false;
  if (new URLPath().StringToIUrl(history.location.hash).details) {
    // setEditMode(true);
    isEditMode = true
  }

  const [isError, setError] = React.useState(false);

  function go(path: string) {
    history.push(path);
  }

  function next() {
    var next = new URLPath().updateFilePath(history.location.hash, relativeObjects.nextFilePath);
    go(next);
  }

  function prev() {
    var prev = new URLPath().updateFilePath(history.location.hash, relativeObjects.prevFilePath);
    go(prev);
  }

  useKeyboardEvent(/Escape/, (event: KeyboardEvent) => {
    var parentDirectory = new URLPath().updateFilePath(history.location.hash, fileIndexItem.parentDirectory);
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

  return (<div className={isEditMode ? "detailview detailview--edit" : "detailview"}>

    {/* {{isLoading ? <Preloader parent={fileIndexItem.parentDirectory} isDetailMenu={true} isOverlay={true}></Preloader> : ""}} */}

    {isEditMode ? <DetailViewSidebar fileIndexItem={fileIndexItem} filePath={fileIndexItem.filePath}></DetailViewSidebar> : ""}

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
