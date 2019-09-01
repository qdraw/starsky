import React, { useEffect } from 'react';
import ArchiveSidebar from '../components/archive-sidebar';
import ItemListView from '../components/item-list-view';
import MenuSearchBar from '../components/menu.searchbar';
import SearchPagination from '../components/search-pagination';
import useLocation from '../hooks/use-location';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { URLPath } from '../shared/url-path';

function Search(archive: IArchiveProps) {

  var history = useLocation();

  // The sidebar
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);

  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar)
  }, [history.location.search]);

  const [query, setQuery] = React.useState(new URLPath().StringToIUrl(history.location.search).t);
  useEffect(() => {
    setQuery(new URLPath().StringToIUrl(history.location.search).t)
  }, [history.location.search]);

  if (!archive) return (<>(Search) => no archive</>)
  if (!archive.colorClassUsage) return (<>(Search) => no colorClassUsage</>)


  return (
    <div className={!sidebar ? "archive" : "archive collapsed"}>
      {sidebar ? <ArchiveSidebar {...archive}></ArchiveSidebar> : ""}

      <div className="content">
        <div className="search-header">
          <MenuSearchBar defaultText={query}></MenuSearchBar>
        </div>
        <div className="content--header">{archive.collectionsCount ? <>{archive.collectionsCount} resultaten</> : "Geen resultaat"}</div>
        <SearchPagination {...archive}></SearchPagination>
        {archive.collectionsCount >= 1 ? <ItemListView {...archive} colorClassUsage={archive.colorClassUsage}> </ItemListView> : null}
        {archive.collectionsCount === 0 ? <div className="folder"><div className="warning-box"> Probeer een andere zoekopdracht</div></div> : null}
        {archive.fileIndexItems.length >= 20 ? <SearchPagination {...archive}></SearchPagination> : null}
      </div>
    </div>
  )
}
export default Search;
