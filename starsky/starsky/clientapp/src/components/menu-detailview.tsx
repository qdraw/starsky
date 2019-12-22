
import { Link } from '@reach/router';
import React, { useEffect, useState } from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import useLocation from '../hooks/use-location';
import { IExifStatus } from '../interfaces/IExifStatus';
import { Orientation } from '../interfaces/IFileIndexItem';
import { CastToInterface } from '../shared/cast-to-interface';
import FetchGet from '../shared/fetch-get';
import FetchPost from '../shared/fetch-post';
import { Keyboard } from '../shared/keyboard';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import ModalDetailviewRenameFile from './modal-detailview-rename-file';
import ModalExport from './modal-export';
import MoreMenu from './more-menu';
import Preloader from './preloader';

const MenuDetailView: React.FunctionComponent = () => {

  var history = useLocation();

  let { state, dispatch } = React.useContext(DetailViewContext);
  var detailView = state;

  var parentDirectory = "/";
  if (detailView && detailView.fileIndexItem && detailView.fileIndexItem.parentDirectory) {
    parentDirectory = detailView.fileIndexItem.parentDirectory;
  }

  const [isDetails, setDetails] = React.useState(new URLPath().StringToIUrl(history.location.search).details);
  useEffect(() => {
    var details = new URLPath().StringToIUrl(history.location.search).details;
    setDetails(details);
    // the state of the image
    setMarkedAsDeleted(getIsMarkedAsDeletedFromProps())
  }, [history.location.search]);

  function toggleLabels() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !isDetails;
    setDetails(urlObject.details);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  // Get the status from the props (null == loading)
  function getIsMarkedAsDeletedFromProps(): boolean {
    if (!detailView) return false;
    return detailView.fileIndexItem.status === IExifStatus.Deleted;
  }
  const [isMarkedAsDeleted, setMarkedAsDeleted] = React.useState(getIsMarkedAsDeletedFromProps());

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  function newBodyParams(): URLSearchParams {
    var bodyParams = new URLSearchParams();
    bodyParams.set("f", detailView.subPath);
    return bodyParams;
  }

  // Trash and Undo Trash
  async function TrashFile() {
    if (!detailView) return;

    setIsLoading(true);
    var bodyParams = newBodyParams();

    // Add remove tag
    if (!isMarkedAsDeleted) {
      bodyParams.set("Tags", "!delete!");
      bodyParams.set("append", "true");
      var resultDo = await FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams.toString());
      if (resultDo.statusCode !== 200) {
        console.error(resultDo);
        setIsLoading(false);
        return;
      }
      dispatch({ 'type': 'append', tags: "!delete!" });
      dispatch({ 'type': 'update', status: IExifStatus.Deleted });
      setMarkedAsDeleted(true);
      setIsLoading(false);
    }
    // Undo trash
    else {
      bodyParams.set("fieldName", "tags");
      bodyParams.set("search", "!delete!");
      var resultUndo = await FetchPost(new UrlQuery().UrlReplaceApi(), bodyParams.toString());
      if (resultUndo.statusCode !== 200) {
        console.error(resultUndo);
        setIsLoading(false);
        return;
      }
      dispatch({ 'type': 'remove', tags: "!delete!" });
      dispatch({ 'type': 'update', status: IExifStatus.Ok });
      setMarkedAsDeleted(false);
      setIsLoading(false);
    }
  }

  /**
   * Checks if the hash is changes and update Context:  orientation + fileHash
   */
  async function requestNewFileHash(): Promise<boolean | null> {
    var resultGet = await FetchGet(new UrlQuery().UrlIndexServerApi({ f: state.subPath }));
    if (resultGet.statusCode !== 200) {
      console.error(resultGet);
      setIsLoading(false);
      return null;
    }
    var media = new CastToInterface().MediaDetailView(resultGet.data).data;
    var orientation = media.fileIndexItem && media.fileIndexItem.orientation ? media.fileIndexItem.orientation : Orientation.Horizontal;

    // the hash changes if you rotate an image
    if (media.fileIndexItem.fileHash === state.fileIndexItem.fileHash) return false;

    dispatch({ 'type': 'update', orientation });
    // triggered on this one
    dispatch({ 'type': 'update', fileHash: media.fileIndexItem.fileHash });
    setIsLoading(false);
    return true;
  }

  /**
   * Update the rotation status
   */
  async function rotateImage90() {
    if (isMarkedAsDeleted) return;
    setIsLoading(true);

    var bodyParams = newBodyParams();
    bodyParams.set("rotateClock", "1");
    var resultPost = await FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams.toString());
    if (resultPost.statusCode !== 200) {
      console.error(resultPost);
      return;
    }

    // there is an async backend event triggered, sometimes there is an que
    setTimeout(async () => {
      var result = await requestNewFileHash();
      if (result === false) {
        setTimeout(async () => {
          await requestNewFileHash();
        }, 7000);
      }
    }, 3000);
  }

  useKeyboardEvent(/(Delete)/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    event.preventDefault();
    TrashFile();
  });

  const [isModalExportOpen, setModalExportOpen] = React.useState(false);
  const [isModalRenameFileOpen, setModalRenameFileOpen] = React.useState(false);

  return (<>
    {isLoading ? <Preloader isDetailMenu={false} isOverlay={true} /> : ""}

    {isModalExportOpen && detailView ? <ModalExport handleExit={() => setModalExportOpen(!isModalExportOpen)} select={[detailView.subPath]} isOpen={isModalExportOpen} /> : null}
    {isModalRenameFileOpen && detailView ? <ModalDetailviewRenameFile handleExit={() => setModalRenameFileOpen(!isModalRenameFileOpen)} isOpen={isModalRenameFileOpen} /> : null}

    <header className={isDetails ? isMarkedAsDeleted ? "header header--main header--edit header--deleted" : "header header--main header--edit" :
      isMarkedAsDeleted ? "header header--main header--deleted" : "header header--main"}>
      <div className="wrapper">
        <Link className="item item--first item--close" to={new URLPath().updateFilePath(history.location.search, parentDirectory)}>Sluiten</Link>
        <div className="item item--labels" onClick={() => { toggleLabels() }}>Labels</div>
        <MoreMenu>
          <li className="menu-option" data-test="export" onClick={() => setModalExportOpen(!isModalExportOpen)}>Exporteer</li>
          {!isDetails ? <li className="menu-option" data-test="labels" onClick={toggleLabels}>Labels</li> : null}
          <li className="menu-option disabled" data-test="move" onClick={() => { alert("werkt nog niet"); }}>Verplaats</li>
          <li className="menu-option" data-test="rename" onClick={() => setModalRenameFileOpen(!isModalRenameFileOpen)}>Naam wijzigen</li>
          <li className="menu-option" data-test="trash" onClick={TrashFile}>{!isMarkedAsDeleted ? "Verplaats naar prullenmand" : "Zet terug uit prullenmand"}</li>
          <li className="menu-option" data-test="rotate" onClick={rotateImage90}>Roteer naar rechts</li>
        </MoreMenu>
      </div>
    </header>

    {isDetails ? <div className="header header--sidebar">
      <div className="item item--close" onClick={() => { toggleLabels(); }}>Sluit detailscherm</div>
    </div> : ""}

  </>);
};

export default MenuDetailView

