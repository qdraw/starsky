
import { Link } from '@reach/router';
import React, { useEffect } from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import useLocation from '../hooks/use-location';
import { IExifStatus } from '../interfaces/IExifStatus';
import FetchPost from '../shared/fetch-post';
import { Keyboard } from '../shared/keyboard';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import ModalDetailviewRenameFile from './modal-detailview-rename-file';
import ModalExport from './modal-export';
import MoreMenu from './more-menu';

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
  }, [history.location.search]);

  function toggleLabels() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !isDetails;
    setDetails(urlObject.details);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  // Get the status from the props (null == loading)
  function getIsMarkedAsDeletedFromProps(): boolean | null {
    if (!detailView) return false;
    return detailView.fileIndexItem.status === IExifStatus.Deleted;
  }

  const [isMarkedAsDeleted, setMarkedAsDeleted] = React.useState(getIsMarkedAsDeletedFromProps());

  // Trash and Undo Trash
  async function TrashFile() {
    if (!detailView) return;

    setMarkedAsDeleted(null);

    var bodyParams = new URLSearchParams();
    bodyParams.set("f", detailView.subPath);

    // Add remove tag
    if (!isMarkedAsDeleted) {
      bodyParams.set("Tags", "!delete!");
      bodyParams.set("append", "true");
      var resultDo = await FetchPost(new UrlQuery().UrlQueryUpdateApi(), bodyParams.toString());
      if (resultDo.statusCode !== 200) {
        console.error(resultDo);
        return;
      }
      dispatch({ 'type': 'append', tags: "!delete!" });
      dispatch({ 'type': 'update', status: IExifStatus.Deleted });
      setMarkedAsDeleted(true);
    }
    // Undo trash
    else {
      bodyParams.set("fieldName", "tags");
      bodyParams.set("search", "!delete!");
      var resultUndo = await FetchPost(new UrlQuery().UrlReplaceApi(), bodyParams.toString());
      if (resultUndo.statusCode !== 200) {
        console.error(resultUndo);
        return;
      }
      dispatch({ 'type': 'remove', tags: "!delete!" });
      dispatch({ 'type': 'update', status: IExifStatus.Ok });
      setMarkedAsDeleted(false);
    }
  }

  useKeyboardEvent(/(Delete)/, (event: KeyboardEvent) => {
    event.preventDefault();
    if (new Keyboard().isInForm(event)) return;
    TrashFile();
  });

  var headerName = isDetails ? "header header--main header--edit" : "header header--main";
  if (isMarkedAsDeleted) headerName += " " + "header--deleted";
  if (isMarkedAsDeleted === null) headerName += " " + "header--loading";

  const [isModalExportOpen, setModalExportOpen] = React.useState(false);
  const [isModalRenameFileOpen, setModalRenameFileOpen] = React.useState(false);

  return (<>
    {isModalExportOpen && detailView ? <ModalExport handleExit={() => setModalExportOpen(!isModalExportOpen)} select={[detailView.subPath]} isOpen={isModalExportOpen} /> : null}
    {isModalRenameFileOpen ? <ModalDetailviewRenameFile handleExit={() => setModalRenameFileOpen(!isModalRenameFileOpen)} isOpen={isModalRenameFileOpen} /> : null}

    <header className={headerName}>
      <div className="wrapper">
        <Link className="item item--first item--close" to={new URLPath().updateFilePath(history.location.search, parentDirectory)}>Sluiten</Link>
        <div className="item item--labels" onClick={() => { toggleLabels() }}>Labels</div>
        <MoreMenu>
          <li className="menu-option" data-test="export" onClick={() => setModalExportOpen(!isModalExportOpen)}>Exporteer</li>
          {!isDetails ? <li className="menu-option" data-test="labels" onClick={() => { toggleLabels() }}>Labels</li> : null}
          <li className="menu-option disabled" data-test="move" onClick={() => { alert("werkt nog niet"); }}>Verplaats</li>
          <li className="menu-option" data-test="rename" onClick={() => setModalRenameFileOpen(!isModalRenameFileOpen)}>Naam wijzigen</li>
          <li className="menu-option" data-test="trash" onClick={() => { TrashFile(); }}>{!isMarkedAsDeleted ? "Verplaats naar prullenmand" : "Zet terug uit prullenmand"}</li>
          <li className="menu-option disabled" data-test="rotate" onClick={() => { alert("werkt nog niet"); }}>Roteer naar rechts</li>
        </MoreMenu>
      </div>
    </header>

    {isDetails ? <div className="header header--sidebar">
      <div className="item item--close" onClick={() => { toggleLabels(); }}>Sluit detailscherm</div>
    </div> : ""}

  </>);
};

export default MenuDetailView

