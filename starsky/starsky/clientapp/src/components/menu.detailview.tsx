
import { Link } from '@reach/router';
import React, { memo, useEffect } from 'react';
import { DetailViewContext } from '../contexts/detailview-context';
import useLocation from '../hooks/use-location';
import { IExifStatus } from '../interfaces/IExifStatus';
import { IMenuProps } from '../interfaces/IMenuProps';
import FetchPost from '../shared/fetchpost';
import { URLPath } from '../shared/url-path';
import MoreMenu from './more-menu';

const MenuDetailView: React.FunctionComponent<IMenuProps> = memo((props) => {

  var history = useLocation();
  let { state, dispatch } = React.useContext(DetailViewContext);

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

  // Get Close url
  const parentUrl = props.parent ? new URLPath().updateFilePath(history.location.search, props.parent) : "/";

  // Get the status from the props
  function getIsMarkedAsDeletedFromProps(): boolean {
    if (!props.detailView || !props.detailView.fileIndexItem) return false;
    return props.detailView.fileIndexItem.status === IExifStatus.Deleted;
  }

  const [isMarkedAsDeleted, setMarkedAsDeleted] = React.useState(getIsMarkedAsDeletedFromProps());

  // update props after a file change
  useEffect(() => {
    setMarkedAsDeleted(getIsMarkedAsDeletedFromProps())
  }, [props.detailView]);

  // Delete and Undo Delete
  function DeleteFile() {
    if (!props.detailView) return;

    var bodyParams = new URLSearchParams();
    bodyParams.set("f", props.detailView.subPath);

    // Add remove tag
    if (!isMarkedAsDeleted) {
      bodyParams.set("Tags", "!delete!");
      bodyParams.set("append", "true");
      FetchPost("/api/update", bodyParams.toString())
      dispatch({ 'type': 'add', tags: "!delete!" });
      dispatch({ 'type': 'update', status: IExifStatus.Deleted });
    }
    // Undo delete
    else {
      bodyParams.set("fieldName", "tags");
      bodyParams.set("search", "!delete!");
      FetchPost("/api/replace", bodyParams.toString())
      dispatch({ 'type': 'remove', tags: "!delete!" });
      dispatch({ 'type': 'update', status: IExifStatus.Ok });
    }
    setMarkedAsDeleted(!isMarkedAsDeleted)
  }

  var headerName = isDetails ? "header header--main header--edit" : "header header--main";
  if (isMarkedAsDeleted) headerName += " " + "header--deleted"

  return (<>
    <header className={headerName}>
      <div className="wrapper">
        <Link className="item item--first item--close" to={parentUrl}>Sluiten</Link>
        <div className="item item--labels" onClick={() => { toggleLabels() }}>Labels</div>
        <MoreMenu>
          <li className="menu-option disabled" onClick={() => { alert("Exporteer werkt nog niet"); }}>Exporteer</li>
          <li className="menu-option disabled" onClick={() => { alert("werkt nog niet"); }}>Verplaats</li>
          <li className="menu-option disabled" onClick={() => { alert("werkt nog niet"); }}>Naam wijzigen</li>
          <li className="menu-option" onClick={() => { DeleteFile(); }}>{!isMarkedAsDeleted ? "Weggooien" : "Undo Weggooien"}</li>
          <li className="menu-option disabled" onClick={() => { alert("werkt nog niet"); }}>Roteer naar rechts</li>
        </MoreMenu>
      </div>
    </header>

    {isDetails ? <div className="header header--sidebar">
      <div className="item item--close" onClick={() => { toggleLabels(); }}>Sluit detailscherm</div>
    </div> : ""}

  </>);
});

export default MenuDetailView

