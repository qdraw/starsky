
import { Link } from '@reach/router';
import React, { memo } from 'react';
import useLocation from '../hooks/use-location';
import { URLPath } from '../shared/url-path';
import MoreMenu from './more-menu';

export interface IMenuProps {
  isDetailMenu: boolean;
  parent?: string;
}

const MenuDetailView: React.FunctionComponent<IMenuProps> = memo((props) => {

  var history = useLocation();
  const [isDetails, setDetails] = React.useState(new URLPath().StringToIUrl(history.location.search).details);

  function toggleLabels() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !urlObject.details;

    setDetails(urlObject.details);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true })
  }

  // Get Close url
  const parentUrl = props.parent ? new URLPath().updateFilePath("props.searchQuery", props.parent) : "/";

  return (<>
    <header className={isDetails ? "header header--main header--edit" : "header header--main"}>
      <div className="wrapper">
        <Link className="item item--first item--close" to={parentUrl}>Sluiten</Link>
        <div className="item item--labels" onClick={() => { toggleLabels() }}>Labels</div>
        <MoreMenu>
          <li className="menu-option">Werkt nog niet!</li>
          <li className="menu-option" onClick={() => { alert("Exporteer werkt nog niet"); }}>Exporteer</li>
          <li className="menu-option" onClick={() => { alert("werkt nog niet"); }}>Verplaats</li>
          <li className="menu-option" onClick={() => { alert("werkt nog niet"); }}>Naam wijzigen</li>
          <li className="menu-option" onClick={() => { alert("werkt nog niet"); }}>Weggooien</li>
          <li className="menu-option" onClick={() => { alert("werkt nog niet"); }}>Roteer naar rechts</li>
        </MoreMenu>
      </div>
    </header>

    {isDetails ? <div className="header header--sidebar">
      <div className="item item--close" onClick={() => { toggleLabels(); }}>Sluit detailscherm</div>
    </div> : ""}

  </>);
});

export default MenuDetailView

