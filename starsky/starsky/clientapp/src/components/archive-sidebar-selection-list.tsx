import React, { memo, useEffect } from 'react';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { Language } from '../shared/language';
import { Select } from '../shared/select';
import { URLPath } from '../shared/url-path';

interface IDetailViewSidebarSelectionListProps {
  fileIndexItems: Array<IFileIndexItem>,
}

const ArchiveSidebarSelectionList: React.FunctionComponent<IDetailViewSidebarSelectionListProps> = memo((props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageNoneSelected = language.text("Niets geselecteerd", "Nothing selected");
  const MessageAllName = language.text("Alles", "All");

  var history = useLocation();
  const [select, setSelect] = React.useState(new URLPath().StringToIUrl(history.location.search).select);
  useEffect(() => {
    setSelect(new URLPath().StringToIUrl(history.location.search).select);
  }, [history.location.search]);

  var allSelection = () => new Select(select, setSelect, props as IArchiveProps, history).allSelection();
  var undoSelection = () => new Select(select, setSelect, props as IArchiveProps, history).undoSelection();
  var toggleSelection = (item: string) => new Select(select, setSelect, props as IArchiveProps, history).toggleSelection(item);

  // noinspection HtmlUnknownAttribute
  return (<div className="sidebar-selection">
    <div className="content--header content--subheader">
      {!select || select.length !== props.fileIndexItems.length ? <button data-test="allSelection" className="btn btn--default"
        onClick={() => allSelection()}>{MessageAllName}</button> : ""}
      {!select || select.length !== 0 ? <button className="btn btn--default" onClick={() => undoSelection()}>Undo</button> : ""}
    </div>
    <ul>
      {!select || select.length === 0 ? <li className="warning-box">{MessageNoneSelected}</li> : ""}
      {
        select ? select.map((item, index) => (
          <li key={index} ><span onClick={() => toggleSelection(item)} className="close" />{item}</li>
        )) : ""
      }
    </ul>
  </div>);
});
export default ArchiveSidebarSelectionList
