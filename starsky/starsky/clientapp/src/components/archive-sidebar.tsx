import React, { memo } from "react";
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import ArchiveSidebarColorClass from './archive-sidebar-color-class';
import ArchiveSidebarLabelEdit from './archive-sidebar-label-edit';
import ArchiveSidebarSelectionList from './archive-sidebar-selection-list';

interface IDetailViewSidebarProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
  subPath: string;
}

const ArchiveSidebar: React.FunctionComponent<IDetailViewSidebarProps> = memo((archive) => {

  return (<div className="sidebar">
    <div className="content--header">
      Selectie
    </div>
    <ArchiveSidebarSelectionList {...archive}></ArchiveSidebarSelectionList>

    <div className="content--header">
      Labels wijzigingen
    </div>
    <ArchiveSidebarLabelEdit {...archive}></ArchiveSidebarLabelEdit>

    <div className="content--header">
      Kleur-Classificatie
      </div>
    <div className="content--text">
      <ArchiveSidebarColorClass {...archive}></ArchiveSidebarColorClass>
    </div>


  </div>);
});
export default ArchiveSidebar