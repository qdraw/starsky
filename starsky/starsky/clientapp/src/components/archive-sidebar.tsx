import React, { memo, useEffect, useLayoutEffect } from "react";
import { PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import ArchiveSidebarColorClass from './archive-sidebar-color-class';
import ArchiveSidebarLabelEdit from './archive-sidebar-label-edit';
import ArchiveSidebarSelectionList from './archive-sidebar-selection-list';

interface IArchiveSidebarProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
  subPath: string;
  isReadOnly: boolean;
  pageType: PageType;
}

const ArchiveSidebar: React.FunctionComponent<IArchiveSidebarProps> = memo((archive) => {

  /**
   * Scroll lock sidebar for mobile devices
   */
  useEffect(() => {
    document.body.style.top = `-${window.scrollY}px`;
    document.body.classList.add("lock-screen");
    return () => {
      document.body.classList.remove("lock-screen");
      const scrollY = document.body.style.top;
      window.scrollTo(0, parseInt(scrollY || '0') * -1);
      document.body.style.top = '';
    };
  });

  /**
   * to avoid changes in location when scrolling while the sidebar is open
   */
  const listener = (e: Event) => {
    document.body.style.top = `-${window.scrollY}px`;
  }
  useLayoutEffect(() => {
    window.addEventListener("scroll", listener);
    return () => {
      window.removeEventListener("scroll", listener);
    };
  });

  /** to avoid wrong props passed */
  if (archive.pageType === PageType.Loading) {
    return (<div className="sidebar"></div>)
  }

  return (<div className="sidebar">

    {archive.isReadOnly ? <>
      <div className="content--header">Status</div>
      <div className="content content--text">
        <div className="warning-box">Alleen lezen map</div>
      </div> </> : null}

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