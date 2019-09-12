import React, { useEffect } from 'react';
import ArchiveSidebar from '../components/archive-sidebar';
import ItemListView from '../components/item-list-view';
import MenuTrash from '../components/menu-trash';
import SearchPagination from '../components/search-pagination';
import { ArchiveContext } from '../contexts/archive-context';
import useLocation from '../hooks/use-location';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { URLPath } from '../shared/url-path';

function Trash(archive: IArchiveProps) {

  var history = useLocation();

  // The sidebar
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);

  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar)
  }, [history.location.search]);

  // to dynamic update the number of trashed items
  let { state } = React.useContext(ArchiveContext);
  const [collectionsCount, setCollectionsCount] = React.useState(state.collectionsCount);
  useEffect(() => {
    setCollectionsCount(state.collectionsCount)
  }, [history.location.search]);

  if (!archive) return (<>(Search) => no archive</>)
  if (!archive.colorClassUsage) return (<>(Search) => no colorClassUsage</>)

  return (
    <>
      <MenuTrash></MenuTrash>
      <div className={!sidebar ? "archive" : "archive collapsed"}>
        {sidebar ? <ArchiveSidebar {...archive}></ArchiveSidebar> : ""}

        <div className="content">
          <div className="content--header">{collectionsCount !== 0 ? <>{collectionsCount} resultaten</> : "Geen resultaat"}</div>
          <SearchPagination {...archive}></SearchPagination>
          {collectionsCount >= 1 ? <ItemListView {...archive} colorClassUsage={archive.colorClassUsage}> </ItemListView> : null}
          {collectionsCount === 0 ? <div className="folder"><div className="warning-box"> Er staat niks in de prullenmand</div></div> : null}
          {archive.fileIndexItems.length >= 20 ? <SearchPagination {...archive}></SearchPagination> : null}
        </div>
      </div>
    </>
  )
}
export default Trash;
