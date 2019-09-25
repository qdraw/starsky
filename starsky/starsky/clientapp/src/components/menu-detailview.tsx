
import { Link } from '@reach/router';
import React, { useEffect } from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import useLocation from '../hooks/use-location';
import { IExifStatus } from '../interfaces/IExifStatus';
import FetchPost from '../shared/fetch-post';
import { Keyboard } from '../shared/keyboard';
import { URLPath } from '../shared/url-path';
import ModalExport from './modal-export';
import ModalRenameFile from './modal-rename-file';
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
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true })
  }

  // Get the status from the props
  function getIsMarkedAsDeletedFromProps(): boolean {
    if (!detailView) return false;
    return detailView.fileIndexItem.status === IExifStatus.Deleted;
  }

  const [isMarkedAsDeleted, setMarkedAsDeleted] = React.useState(getIsMarkedAsDeletedFromProps());

  // update props after a file change
  useEffect(() => {
    setMarkedAsDeleted(getIsMarkedAsDeletedFromProps())
  }, [detailView]);

  // Trash and Undo Trash
  function TrashFile() {
    if (!detailView) return;

    var bodyParams = new URLSearchParams();
    bodyParams.set("f", detailView.subPath);

    // Add remove tag
    if (!isMarkedAsDeleted) {
      bodyParams.set("Tags", "!delete!");
      bodyParams.set("append", "true");
      FetchPost("/api/update", bodyParams.toString())
      dispatch({ 'type': 'add', tags: "!delete!" });
      dispatch({ 'type': 'update', status: IExifStatus.Deleted });
    }
    // Undo trash
    else {
      bodyParams.set("fieldName", "tags");
      bodyParams.set("search", "!delete!");
      FetchPost("/api/replace", bodyParams.toString())
      dispatch({ 'type': 'remove', tags: "!delete!" });
      dispatch({ 'type': 'update', status: IExifStatus.Ok });
    }
  }

  useKeyboardEvent(/(Delete)/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    TrashFile();
  })

  var headerName = isDetails ? "header header--main header--edit" : "header header--main";
  if (isMarkedAsDeleted) headerName += " " + "header--deleted"

  const [isModalExportOpen, setModalExportOpen] = React.useState(false);
  const [isModalRenameFileOpen, setModalRenameFileOpen] = React.useState(false);

  return (<>
    {isModalExportOpen ? <ModalExport handleExit={() => setModalExportOpen(!isModalExportOpen)} select={[detailView.subPath]} isOpen={isModalExportOpen} /> : null}
    {isModalRenameFileOpen ? <ModalRenameFile handleExit={() => setModalRenameFileOpen(!isModalRenameFileOpen)} isOpen={isModalRenameFileOpen} /> : null}

    <header className={headerName}>
      <div className="wrapper">
        <Link className="item item--first item--close" to={new URLPath().updateFilePath(history.location.search, parentDirectory)}>Sluiten</Link>
        <div className="item item--labels" onClick={() => { toggleLabels() }}>Labels</div>
        <MoreMenu>
          <li className="menu-option" onClick={() => setModalExportOpen(!isModalExportOpen)}>Exporteer</li>
          {!isDetails ? <li className="menu-option" onClick={() => { toggleLabels() }}>Labels</li> : null}
          <li className="menu-option disabled" onClick={() => { alert("werkt nog niet"); }}>Verplaats</li>
          <li className="menu-option disabled" onClick={() => setModalRenameFileOpen(!isModalRenameFileOpen)}>Naam wijzigen</li>
          <li className="menu-option" onClick={() => { TrashFile(); }}>{!isMarkedAsDeleted ? "Verplaats naar prullenmand" : "Zet terug uit prullenmand"}</li>
          <li className="menu-option disabled" onClick={() => { alert("werkt nog niet"); }}>Roteer naar rechts</li>
        </MoreMenu>
      </div>
    </header>

    {isDetails ? <div className="header header--sidebar">
      <div className="item item--close" onClick={() => { toggleLabels(); }}>Sluit detailscherm</div>
    </div> : ""}

  </>);
};

export default MenuDetailView

