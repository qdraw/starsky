import React, { memo, useEffect } from 'react';
import useLocation from '../hooks/use-location';
import { URLPath } from '../shared/url-path';
import ColorClassSelect from './color-class-select';


const ArchiveSidebarColorClass: React.FunctionComponent<any> = memo((props) => {
  var history = useLocation();

  // show select info
  const [selectParams, setSelectParams] = React.useState("");
  useEffect(() => {
    var select = new URLPath().getSelect(history.location.search);
    var path = new URLPath().getFilePath(history.location.search)
    console.log(path);

    var selectParams = new URLPath().ArrayToCommaSeperatedString(select, path)
    setSelectParams(selectParams);
  }, [history.location.search]);

  return (<ColorClassSelect filePath={selectParams} isEnabled={true}></ColorClassSelect>)
});
export default ArchiveSidebarColorClass