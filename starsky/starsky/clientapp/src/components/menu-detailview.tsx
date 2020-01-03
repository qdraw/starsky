
import { Link } from '@reach/router';
import React, { useEffect, useState } from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import useLocation from '../hooks/use-location';
import { IDetailView, PageType } from '../interfaces/IDetailView';
import { IExifStatus } from '../interfaces/IExifStatus';
import { Orientation } from '../interfaces/IFileIndexItem';
import { INavigateState } from '../interfaces/INavigateState';
import { CastToInterface } from '../shared/cast-to-interface';
import { IsEditedNow } from '../shared/date';
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

  // fallback state
  if (!state) {
    state = {
      pageType: PageType.Loading,
      fileIndexItem: {
        parentDirectory: "/",
        fileName: '',
        filePath: "/",
        lastEdited: new Date(1970, 0, 1).toISOString()
      }
    } as IDetailView;
  }

  const [isDetails, setDetails] = React.useState(new URLPath().StringToIUrl(history.location.search).details);
  useEffect(() => {
    var details = new URLPath().StringToIUrl(history.location.search).details;
    setDetails(details);
  }, [history.location.search]);

  // know if you searching ?t= in url
  const [isSearchQuery, setIsSearchQuery] = React.useState(!!new URLPath().StringToIUrl(history.location.search).t);
  useEffect(() => {
    setIsSearchQuery(!!new URLPath().StringToIUrl(history.location.search).t);
  }, [history.location.search]);

  /* only update when the state is changed */
  useEffect(() => {
    setMarkedAsDeleted(getIsMarkedAsDeletedFromProps())
    console.log('status, ', state.fileIndexItem.status);

  }, [state.fileIndexItem.status]);

  /* show marker with 'Saved' */
  const [isRecentEdited, setRecentEdited] = React.useState(IsEditedNow(state.fileIndexItem.lastEdited));
  useEffect(() => {
    if (!state.fileIndexItem.lastEdited) return;
    var isEditedNow = IsEditedNow(state.fileIndexItem.lastEdited);
    console.log('isEditedNow', isEditedNow);

    if (!isEditedNow) {
      setRecentEdited(false);
      return;
    };
    setRecentEdited(isEditedNow);
  }, [state.fileIndexItem.lastEdited]);

  function toggleLabels() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !isDetails;
    setDetails(urlObject.details);
    setRecentEdited(false); // disable to avoid animation
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  // Get the status from the props (null == loading)
  function getIsMarkedAsDeletedFromProps(): boolean {
    if (!state) return false;
    return state.fileIndexItem.status === IExifStatus.Deleted;
  }
  const [isMarkedAsDeleted, setMarkedAsDeleted] = React.useState(getIsMarkedAsDeletedFromProps());

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  /**
   * Create body params to do url queries
   */
  function newBodyParams(): URLSearchParams {
    var bodyParams = new URLSearchParams();
    bodyParams.set("f", state.subPath);
    return bodyParams;
  }

  // Trash and Undo Trash
  async function TrashFile() {
    if (!state) return;

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
      dispatch({ 'type': 'update', status: IExifStatus.Deleted, lastEdited: new Date().toISOString() });
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
      dispatch({ 'type': 'update', status: IExifStatus.Ok, lastEdited: new Date().toISOString() });
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

    dispatch({ 'type': 'update', orientation, fileHash: media.fileIndexItem.fileHash });
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

    {isModalExportOpen && state ? <ModalExport handleExit={() => setModalExportOpen(!isModalExportOpen)} select={[state.subPath]} isOpen={isModalExportOpen} /> : null}
    {isModalRenameFileOpen && state ? <ModalDetailviewRenameFile handleExit={() => setModalRenameFileOpen(!isModalRenameFileOpen)} isOpen={isModalRenameFileOpen} /> : null}

    <header className={isDetails ? isMarkedAsDeleted ? "header header--main header--edit header--deleted" : "header header--main header--edit" :
      isMarkedAsDeleted ? "header header--main header--deleted" : "header header--main"}>
      <div className="wrapper">

        {/* in directory state */}
        {!isSearchQuery ? <Link className="item item--first item--close"
          state={{ filePath: state.fileIndexItem.filePath } as INavigateState}
          to={new URLPath().updateFilePath(history.location.search, state.fileIndexItem.parentDirectory)}>Sluiten</Link> : null}

        {/* to search */}
        {isSearchQuery ? <Link className="item item--first item--search"
          state={{ filePath: state.fileIndexItem.filePath } as INavigateState}
          to={new URLPath().Search(history.location.search)}>{new URLPath().StringToIUrl(history.location.search).t}</Link> : null}

        <div className="item item--labels" onClick={() => { toggleLabels() }}>Labels</div>
        <MoreMenu>
          <li className="menu-option" data-test="export" onClick={() => setModalExportOpen(!isModalExportOpen)}>Download</li>
          {!isDetails ? <li className="menu-option" data-test="labels" onClick={toggleLabels}>Labels</li> : null}
          <li className="menu-option disabled" data-test="move" onClick={() => { alert("werkt nog niet"); }}>Verplaats</li>
          <li className="menu-option" data-test="rename" onClick={() => setModalRenameFileOpen(!isModalRenameFileOpen)}>Naam wijzigen</li>
          <li className="menu-option" data-test="trash" onClick={TrashFile}>{!isMarkedAsDeleted ? "Verplaats naar prullenmand" : "Zet terug uit prullenmand"}</li>
          <li className="menu-option" data-test="rotate" onClick={rotateImage90}>Roteer naar rechts</li>
        </MoreMenu>
      </div>
    </header>

    {isDetails ? <div className="header header--sidebar">
      <div className="item item--close" onClick={() => { toggleLabels(); }}>
        Sluit detailscherm
        {isRecentEdited ? <div className="autosave">Opgeslagen</div> : null}
      </div>
    </div> : ""}

  </>);
};

export default MenuDetailView

