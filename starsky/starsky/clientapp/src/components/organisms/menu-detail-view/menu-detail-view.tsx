
import { Link } from '@reach/router';
import React, { useEffect, useState } from 'react';
import { DetailViewContext } from '../../../contexts/detailview-context';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useKeyboardEvent from '../../../hooks/use-keyboard-event';
import useLocation from '../../../hooks/use-location';
import { IDetailView, PageType } from '../../../interfaces/IDetailView';
import { IExifStatus } from '../../../interfaces/IExifStatus';
import { Orientation } from '../../../interfaces/IFileIndexItem';
import { INavigateState } from '../../../interfaces/INavigateState';
import { CastToInterface } from '../../../shared/cast-to-interface';
import { IsEditedNow } from '../../../shared/date';
import FetchGet from '../../../shared/fetch-get';
import FetchPost from '../../../shared/fetch-post';
import { Keyboard } from '../../../shared/keyboard';
import { Language } from '../../../shared/language';
import { URLPath } from '../../../shared/url-path';
import { UrlQuery } from '../../../shared/url-query';
import MoreMenu from '../../atoms/more-menu/more-menu';
import Preloader from '../../atoms/preloader/preloader';
import ModalDetailviewRenameFile from '../modal-detailview-rename-file/modal-detailview-rename-file';
import ModalDownload from '../modal-download/modal-download';
import ModalMoveFile from '../modal-move-file/modal-move-file';

const MenuDetailView: React.FunctionComponent = () => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageCloseDialog = language.text("Sluiten", "Close");
  const MessageCloseDetailScreenDialog = language.text("Sluit detailscherm", "Close detail screen");
  const MessageSaved = language.text("Opgeslagen", "Saved");
  const MessageMoveToTrash = language.text("Verplaats naar prullenmand", "Move to Trash");
  const MessageRestoreFromTrash = language.text("Zet terug uit prullenmand", "Restore from Trash");
  const MessageMove = language.text("Verplaats", "Move");
  const MessageRenameFileName = language.text("Bestandsnaam wijzigen", "Rename file name");
  const MessageRotateToRight = language.text("Rotatie naar rechts", "Rotation to the right");
  const MessageGoToParentFolder = language.text("Ga naar bovenliggende map", "Go to parent folder");

  var history = useLocation();

  let { state, dispatch } = React.useContext(DetailViewContext);

  // fallback state
  if (!state) {
    state = {
      pageType: PageType.Loading,
      isReadOnly: true,
      fileIndexItem: {
        parentDirectory: "/",
        fileName: '',
        filePath: "/",
        lastEdited: new Date(1970, 1, 1).toISOString(),
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

  /* show marker with 'Saved' */
  const [isRecentEdited, setRecentEdited] = React.useState(IsEditedNow(state.fileIndexItem.lastEdited));
  useEffect(() => {
    if (!state.fileIndexItem.lastEdited) return;
    const isEditedNow = IsEditedNow(state.fileIndexItem.lastEdited);
    if (!isEditedNow) {
      setRecentEdited(false);
      return;
    }
    setRecentEdited(isEditedNow);
  }, [state.fileIndexItem.lastEdited]);

  function toggleLabels() {
    const urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !isDetails;
    setDetails(urlObject.details);
    setRecentEdited(false); // disable to avoid animation
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  const [isMarkedAsDeleted, setMarkedAsDeleted] = React.useState(state.fileIndexItem.status === IExifStatus.Deleted);

  /* only update when the state is changed */
  useEffect(() => {
    setMarkedAsDeleted(state.fileIndexItem.status === IExifStatus.Deleted);
  }, [state.fileIndexItem.status]);

  /* only update when the state is changed */
  const [isReadOnly, setReadOnly] = React.useState(state.isReadOnly);
  useEffect(() => {
    setReadOnly(state.isReadOnly);
  }, [state.isReadOnly]);

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
    if (!state || isReadOnly) return;

    setIsLoading(true);
    var bodyParams = newBodyParams();

    // Add remove tag
    if (!isMarkedAsDeleted) {
      bodyParams.set("Tags", "!delete!");
      bodyParams.set("append", "true");
      var resultDo = await FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams.toString());
      if (resultDo.statusCode !== 200 && resultDo.statusCode !== 404) {
        // 404: file can already been deleted
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
    clearSearchCache();
  }

  function clearSearchCache() {
    // clear search cache * when you refresh the search page this is needed to display the correct labels
    var searchTag = new URLPath().StringToIUrl(history.location.search).t;
    if (!searchTag) return;
    FetchPost(new UrlQuery().UrlSearchRemoveCacheApi(), `t=${searchTag}`);
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
    if (isMarkedAsDeleted || isReadOnly) return;
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
          // when it didn't change after two tries
          setIsLoading(false);
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
  const [isModalMoveFile, setModalMoveFile] = React.useState(false);

  const goToParentFolderJSX: JSX.Element | null = isSearchQuery ? <li className="menu-option" data-test="go-to-parent-folder" onClick={() =>
    history.navigate(new UrlQuery().updateFilePathHash(history.location.search, state.fileIndexItem.parentDirectory, true), {
      state: {
        filePath: state.fileIndexItem.filePath
      } as INavigateState
    })}>
    {MessageGoToParentFolder}
  </li > : null

  return (<>
    {isLoading ? <Preloader isDetailMenu={false} isOverlay={true} /> : ""}

    {/* allowed in readonly to download */}
    {isModalExportOpen && state ? <ModalDownload collections={false} handleExit={() => setModalExportOpen(!isModalExportOpen)}
      select={[state.subPath]} isOpen={isModalExportOpen} /> : null}
    {isModalRenameFileOpen && state && !isReadOnly ? <ModalDetailviewRenameFile handleExit={() => setModalRenameFileOpen(!isModalRenameFileOpen)}
      isOpen={isModalRenameFileOpen} /> : null}
    {isModalMoveFile && state && !isReadOnly ? <ModalMoveFile selectedSubPath={state.fileIndexItem.filePath}
      parentDirectory={state.fileIndexItem.parentDirectory} handleExit={() => setModalMoveFile(!isModalMoveFile)}
      isOpen={isModalMoveFile} /> : null}

    <header className={isDetails ? isMarkedAsDeleted ? "header header--main header--edit header--deleted" : "header header--main header--edit" :
      isMarkedAsDeleted ? "header header--main header--deleted" : "header header--main"}>
      <div className="wrapper">

        {/* in directory state aka no search */}
        {!isSearchQuery ? <Link className="item item--first item--close"
          state={{ filePath: state.fileIndexItem.filePath } as INavigateState}
          onClick={() => { setIsLoading(true) }}
          to={new UrlQuery().updateFilePathHash(history.location.search, state.fileIndexItem.parentDirectory)}>{MessageCloseDialog}</Link> : null}

        {/* to search */}
        {isSearchQuery ? <Link className="item item--first item--search"
          state={{ filePath: state.fileIndexItem.filePath } as INavigateState}
          to={new UrlQuery().HashSearchPage(history.location.search)}>{new URLPath().StringToIUrl(history.location.search).t}</Link> : null}

        <div className="item item--labels" onClick={() => { toggleLabels() }}>Labels</div>
        <MoreMenu>
          {goToParentFolderJSX}
          <li className="menu-option" data-test="export" onClick={() => setModalExportOpen(!isModalExportOpen)}>Download</li>
          {!isDetails ? <li className="menu-option" data-test="labels" onClick={toggleLabels}>Labels</li> : null}
          <li className={!isReadOnly ? "menu-option" : "menu-option disabled"} data-test="move" onClick={() => setModalMoveFile(!isModalMoveFile)}>{MessageMove}</li>
          <li className={!isReadOnly ? "menu-option" : "menu-option disabled"} data-test="rename" onClick={() => setModalRenameFileOpen(!isModalRenameFileOpen)}>
            {MessageRenameFileName}</li>
          <li className={!isReadOnly ? "menu-option" : "menu-option disabled"} data-test="trash" onClick={TrashFile}>
            {!isMarkedAsDeleted ? MessageMoveToTrash : MessageRestoreFromTrash}</li>
          <li className={!isReadOnly ? "menu-option" : "menu-option disabled"} data-test="rotate" onClick={rotateImage90}>{MessageRotateToRight}</li>
        </MoreMenu>
      </div>
    </header>

    {isDetails ? <div className="header header--sidebar">
      <div className="item item--close" onClick={() => { toggleLabels(); }}>
        {MessageCloseDetailScreenDialog}
        {isRecentEdited ? <div className="autosave">{MessageSaved}</div> : null}
      </div>
    </div> : ""}

  </>);
};

export default MenuDetailView

