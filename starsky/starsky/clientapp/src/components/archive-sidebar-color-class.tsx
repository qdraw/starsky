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

/**
 * Use for updating/writing multiple files with one colorClass label
 */
const ArchiveSidebarColorClass: React.FunctionComponent<IArchiveSidebarColorClassProps> = memo((props) => {
  const history = useLocation();

  // show select info
  const [select, setSelect] = React.useState(new URLPath().getSelect(history.location.search));
  useEffect(() => {
    setSelect(new URLPath().getSelect(history.location.search));
  }, [history.location.search]);

  // updated parameters based on select
  const [selectParams, setSelectParams] = React.useState("");
  useEffect(() => {
    var subPaths = new URLPath().MergeSelectFileIndexItem(select, props.fileIndexItems);
    var selectParamsLocal = new URLPath().ArrayToCommaSeperatedStringOneParent(subPaths, "")
    setSelectParams(selectParamsLocal);
  }, [select]);

  let { dispatch } = React.useContext(ArchiveContext);

  return (<ColorClassSelect onToggle={(colorclass) => {
      dispatch({type: 'update', colorclass, select});
    }} 
    filePath={selectParams} isEnabled={!props.isReadOnly && select.length !== 0} clearAfter={true} />)
});
export default ArchiveSidebarColorClass
