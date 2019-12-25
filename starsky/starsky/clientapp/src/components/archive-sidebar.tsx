import React, { memo, useEffect, useLayoutEffect } from "react";
import useLocation from '../hooks/use-location';
import { PageType } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';
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

  // Update view based on url parameters
  var history = useLocation();
  const [isSidebar, setIsSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);
  useEffect(() => {
    var sidebarLocal = new URLPath().StringToIUrl(history.location.search).sidebar;
    setIsSidebar(sidebarLocal);
  }, [history.location.search]);

  /** 
   * toggle when activate &sidebar=true 
   */
  useEffect(() => {
    if (isSidebar) {
      document.body.classList.add("lock-screen");
    }
    else {
      document.body.classList.remove("lock-screen");
      window.scrollTo(0, parseInt(document.body.style.top || '0') * -1);
    }
    return () => {
      document.body.classList.remove("lock-screen");
      window.scrollTo(0, parseInt(document.body.style.top || '0') * -1);
    };
  }, [isSidebar]);

  /**
   * to avoid changes in location when scrolling while the sidebar is open
   */
  const listener = (e: Event) => {
    if (!window.scrollY) return;
    document.body.style.top = `-${window.scrollY}px`;
  };

  useLayoutEffect(() => {
    window.addEventListener("scroll", listener);
    return () => {
      window.removeEventListener("scroll", listener);
    };
  });

  /** to avoid wrong props passed */
  if (archive.pageType === PageType.Loading) {
    return (<div className="sidebar" />)
  }

  if (!isSidebar) {
    return <></>
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
    <ArchiveSidebarSelectionList {...archive} />

    <div className="content--header">
      Labels wijzigingen
    </div>
    <ArchiveSidebarLabelEdit {...archive} />

    <div className="content--header">
      Kleur-Classificatie
      </div>
    <div className="content--text">
      <ArchiveSidebarColorClass {...archive} />
    </div>
  </div>);
});

export default ArchiveSidebar
