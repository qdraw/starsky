import React, { memo, useEffect } from 'react';
import useLocation from '../hooks/use-location';
import { URLPath } from '../shared/url-path';


const ArchiveSidebarSelectionList: React.FunctionComponent = memo((props) => {
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


  return (<div className="sidebar-selection">
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