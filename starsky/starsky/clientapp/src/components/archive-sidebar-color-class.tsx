import React, { memo, useEffect } from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import useLocation from '../hooks/use-location';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';
import ColorClassSelect from './color-class-select';

interface IArchiveSidebarColorClassProps {
  fileIndexItems: Array<IFileIndexItem>,
  isReadOnly: boolean,
}

const ArchiveSidebarColorClass: React.FunctionComponent<IArchiveSidebarColorClassProps> = memo((archive) => {
  var history = useLocation();

  // show select info
  const [selectParams, setSelectParams] = React.useState("");
  const [select, setSelect] = React.useState(new URLPath().getSelect(history.location.search));

  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
    var subPaths = new URLPath().MergeSelectFileIndexItem(select, archive.fileIndexItems);
    var selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(subPaths, "")
    setSelectParams(selectParams);
  }, [history.location.search]);

  let { dispatch } = React.useContext(ArchiveContext);

  return (<ColorClassSelect onToggle={(colorclass) => {
    dispatch({ type: 'update', colorclass, select });
  }} filePath={selectParams} isEnabled={!archive.isReadOnly && select.length !== 0} clearAfter={true} ></ColorClassSelect>)
});
export default ArchiveSidebarColorClass