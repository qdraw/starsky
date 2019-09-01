import React, { useEffect } from 'react';
import ItemListView from '../components/item-list-view';
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

  if (!archive) return (<>(Search) => no archive</>)
  if (!archive.colorClassUsage) return (<>(Search) => no colorClassUsage</>)

  return (
    <div className={!sidebar ? "archive" : "archive collapsed"}>
      {/* {sidebar ? <ArchiveSidebar {...archive}></ArchiveSidebar> : ""} */}

      <div className="content">
        <SearchPagination {...archive}></SearchPagination>
        <ItemListView {...archive} colorClassUsage={archive.colorClassUsage}> </ItemListView>
      </div>
    </div>
  )
}
export default Search;
