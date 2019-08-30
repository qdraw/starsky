import React, { memo, useEffect } from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import useLocation from '../hooks/use-location';
import { URLPath } from '../shared/url-path';
import ColorClassSelect from './color-class-select';


const ArchiveSidebarColorClass: React.FunctionComponent<any> = memo((props) => {
  var history = useLocation();

  // show select info
  const [selectParams, setSelectParams] = React.useState("");
  const [select, setSelect] = React.useState(new URLPath().getSelect(history.location.search));

  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
    var path = new URLPath().getFilePath(history.location.search)
    console.log(path);

    var selectParams = new URLPath().ArrayToCommaSeperatedString(select, path)
    setSelectParams(selectParams);
  }, [history.location.search]);

  let { state, dispatch } = React.useContext(ArchiveContext);

  return (<ColorClassSelect onToggle={(colorclass) => {
    dispatch({ type: 'update', colorclass, select });

  }} filePath={selectParams} isEnabled={true} clearAfter={true} ></ColorClassSelect>)
});
export default ArchiveSidebarColorClass