import React, { memo, useEffect } from 'react';
import useLocation from '../hooks/use-location';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';

interface IDetailViewSidebarSelectionListProps {
  fileIndexItems: Array<IFileIndexItem>,
}

const ArchiveSidebarSelectionList: React.FunctionComponent<IDetailViewSidebarSelectionListProps> = memo((props) => {
  var isEnabled = true;

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

  function allSelection() {
    if (!select) return;

    props.fileIndexItems.forEach(fileIndexItem => {
      var include = select.includes(fileIndexItem.fileName);
      if (!include) {
        select.push(fileIndexItem.fileName)
      }
    });

    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.select = select;
    setSelect(urlObject.select);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  function undoSelection() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.select = [];
    setSelect(urlObject.select);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  return (<div className="sidebar-selection">
    <div className="content--header content--header--bluegray50">
      {!select || select.length !== props.fileIndexItems.length ? <a className="btn btn--default" onClick={() => allSelection()}>Alles</a> : ""}

      {!select || select.length !== 0 ? <a className="btn btn--default" onClick={() => undoSelection()}>Undo</a> : ""}
    </div>
    <ul>
      {!select || select.length === 0 ? <li className="warning-box">Niks geselecteerd</li> : ""}

      {
        select ? select.map((item, index) => (
          <li key={index} ><span onClick={() => toggleSelection(item)} className="close"></span>{item}</li>
        )) : ""
      }
    </ul>

  </div>);
});
export default ArchiveSidebarSelectionList