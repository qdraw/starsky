
import React, { memo } from 'react';
import { URLPath } from '../shared/url-path';
import Link from './Link';
import MoreMenu from './more-menu';

export interface IMenuProps {
  isDetailMenu: boolean;
  parent?: string;
}

const MenuDetailView: React.FunctionComponent<IMenuProps> = memo((props) => {

  function toggle() {
    var urlObject = new URLPath().StringToIUrl("props.searchQuery")
    urlObject.details = !isEditMode;
    throw Error("toggle");
    // props.searchQuery.replace(new URLPath().IUrlToString(urlObject))
  }

  // Get Close url
  const parentUrl = props.parent ? new URLPath().updateFilePath("props.searchQuery", props.parent) : "/";

  // Details-mode
  const [isEditMode, setEditMode] = React.useState(new URLPath().StringToIUrl("props.searchQuery").details);

  return (<>

    <header className={isEditMode ? "header header--main header--edit" : "header header--main"}>
      <div className="wrapper">
        <Link className="item item--first item--close" href={parentUrl}>Sluiten</Link>
        <div className="item item--labels" onClick={() => { toggle() }}>Labels</div>
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



    {isEditMode ? <div className="header header--sidebar">
      <div className="item item--close" onClick={() => { toggle(); }}>Sluiten</div>
    </div> : ""}


  </>);
});

export default MenuDetailView

