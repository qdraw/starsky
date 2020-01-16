import React, { useEffect } from 'react';
import ArchiveSidebar from '../components/archive-sidebar';
import Breadcrumb from '../components/breadcrumbs';
import ColorClassFilter from '../components/color-class-filter';
import ItemListView from '../components/item-list-view';
import MenuArchive from '../components/menu-archive';
import RelativeLink from '../components/relative-link';
import useLocation from '../hooks/use-location';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { URLPath } from '../shared/url-path';


function Archive(archive: IArchiveProps) {

  var history = useLocation();

  // The sidebar
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);
  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar)
  }, [history.location.search]);

  if (archive && (!archive.colorClassUsage || !archive.colorClassFilterList)) return (<>(Archive) => no colorClassLists</>)

  return (
    <>
      <MenuArchive />
      <div className={!sidebar ? "archive" : "archive collapsed"}>
        <ArchiveSidebar {...archive} />

        <div className="content">
          <Breadcrumb breadcrumb={archive.breadcrumb} subPath={archive.subPath} />
          <RelativeLink relativeObjects={archive.relativeObjects} />

          <ColorClassFilter itemsCount={archive.collectionsCount} subPath={archive.subPath}
            colorClassFilterList={archive.colorClassFilterList}
            colorClassUsage={archive.colorClassUsage} />
          <ItemListView {...archive}> </ItemListView>
        </div>
      </div>
    </>
  )
}
export default Archive;
