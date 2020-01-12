import React, { memo, useEffect } from 'react';
import useLocation from '../hooks/use-location';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';

interface IDetailViewSidebarSelectionListProps {
  fileIndexItems: Array<IFileIndexItem>,
}

const ArchiveSidebarSelectionList: React.FunctionComponent<IDetailViewSidebarSelectionListProps> = memo((props) => {

  var history = useLocation();
  const [select, setSelect] = React.useState(new URLPath().StringToIUrl(history.location.search).select);
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select);
  }, [history.location.search]);

  function toggleSelection(fileName: string): void {
    var urlObject = new URLPath().toggleSelection(fileName, history.location.search);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
    setSelect(urlObject.select);
  }

  // Select All items
  function allSelection() {
    if (!select) return;
    var updatedSelect = new URLPath().GetAllSelection(select, props.fileIndexItems);
    var urlObject = new URLPath().updateSelection(history.location.search, updatedSelect);
    setSelect(urlObject.select);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  // Undo Selection
  function undoSelection() {
    var urlObject = new URLPath().updateSelection(history.location.search, []);
    setSelect(urlObject.select);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  // noinspection HtmlUnknownAttribute
  return (<div className="sidebar-selection">
    <div className="content--header content--subheader">
      {!select || select.length !== props.fileIndexItems.length ? <button data-test="allSelection" className="btn btn--default" onClick={() => allSelection()}>Alles</button> : ""}
      {!select || select.length !== 0 ? <button className="btn btn--default" onClick={() => undoSelection()}>Undo</button> : ""}
    </div>
    <ul>
      {!select || select.length === 0 ? <li className="warning-box">Niets geselecteerd</li> : ""}

      {
        select ? select.map((item, index) => (
          <li key={index} ><span onClick={() => toggleSelection(item)} className="close" />{item}</li>
        )) : ""
      }
    </ul>
  </div>);
});
export default ArchiveSidebarSelectionList
