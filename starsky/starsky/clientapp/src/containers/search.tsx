import React, { useEffect } from 'react';
import ArchiveSidebar from '../components/archive-sidebar';
import ItemListView from '../components/item-list-view';
import MenuSearch from '../components/menu-search';
import MenuSearchBar from '../components/menu.searchbar';
import SearchPagination from '../components/search-pagination';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { Language } from '../shared/language';
import { URLPath } from '../shared/url-path';

function Search(archive: IArchiveProps) {

  // Content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageNumberOfResults = language.text("resultaten", "results");
  const MessageNoResult = language.text("Geen resultaat", "No result");
  const MessageTryOtherQuery = language.text("Probeer een andere zoekopdracht", "Try another search query");

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

  if (!archive) return (<>(Search) => no archive</>);
  if (!archive.colorClassUsage) return (<>(Search) => no colorClassUsage</>);

  return (<>
    <MenuSearch />
    <div className={!sidebar ? "archive" : "archive collapsed"}>
      <ArchiveSidebar {...archive} collections={false} />
      <div className="content">
        <div className="search-header">
          <MenuSearchBar defaultText={query} />
        </div>
        <div className="content--header">
          {archive.collectionsCount ? <>
            {archive.collectionsCount} {MessageNumberOfResults}
          </> : MessageNoResult}
        </div>
        <SearchPagination {...archive} />
        {archive.collectionsCount >= 1 ? <ItemListView {...archive} colorClassUsage={archive.colorClassUsage}>
        </ItemListView> : null}
        {archive.collectionsCount === 0 ? <div className="folder">
          <div className="warning-box">
            {MessageTryOtherQuery}
          </div>
        </div> : null}
        {archive.fileIndexItems.length >= 20 ? <SearchPagination {...archive} /> : null}
      </div>
    </div>
  </>
  )
}
export default Search;
