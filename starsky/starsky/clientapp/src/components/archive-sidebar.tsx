import React, { memo } from "react";
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import ArchiveSidebarSelectionList from './archive-sidebar-selection-list';

interface IDetailViewSidebarProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
}

const ArchiveSidebar: React.FunctionComponent<IDetailViewSidebarProps> = memo((archive) => {
  var isEnabled = true;

  return (<div className="sidebar">
    <div className="content--header">
      Selectie
    </div>
    <ArchiveSidebarSelectionList {...archive}></ArchiveSidebarSelectionList>

    <div className="content--header">
      Labels wijzigingen
    </div>
    <div className="content--text">
      <div data-name="tags"
        suppressContentEditableWarning={true}
        contentEditable={isEnabled}
        className={isEnabled ? "form-control" : "form-control disabled"}>
      </div>
    </div>

    <div className="content--header">
      Info &amp; Titel
      </div>
    <div className="content--text">
      <h4>Info</h4>
      <div
        data-name="description"
        suppressContentEditableWarning={true}
        contentEditable={isEnabled}
        className={isEnabled ? "form-control" : "form-control disabled"}>
      </div>
      <h4>Titel</h4>
      <div data-name="title"
        suppressContentEditableWarning={true}
        contentEditable={isEnabled}
        className={isEnabled ? "form-control" : "form-control disabled"}>
      </div>
    </div>

    <div className="content--header">
      Kleur-Classificatie
      </div>
    <div className="content--text">
      {/* <ColorClassSelect filePath={fileIndexItem.filePath} currentColorClass={fileIndexItem.colorClass} isEnabled={isEnabled}></ColorClassSelect> */}
    </div>


  </div>);
});
export default ArchiveSidebar