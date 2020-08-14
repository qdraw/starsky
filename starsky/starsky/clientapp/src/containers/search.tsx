import React, { useEffect } from 'react';
import ItemListView from '../components/molecules/item-list-view/item-list-view';
import MenuSearchBar from '../components/molecules/menu-inline-search/menu-inline-search';
import SearchPagination from '../components/molecules/search-pagination/search-pagination';
import ArchiveSidebar from '../components/organisms/archive-sidebar/archive-sidebar';
import MenuSearch from '../components/organisms/menu-search/menu-search';
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
  const MessagePageNumberToken = language.text("Pagina {pageNumber} van ", "Page {pageNumber} of ") // space at end

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

  if (!archive) return (<>(Search) no archive</>);
  if (!archive.colorClassUsage) return (<>(Search) no colorClassUsage</>);

  return (<>
    <MenuSearch />
    <div className={!sidebar ? "archive" : "archive collapsed"}>
      <ArchiveSidebar {...archive} />
      <div className="content">
        <div className="search-header">
          <MenuSearchBar defaultText={query} />
        </div>
        <div className="content--header">
          {!archive.collectionsCount ? MessageNoResult : null}
          {archive.collectionsCount && archive.pageNumber === 0 ? <>
            {archive.collectionsCount} {MessageNumberOfResults}
          </> : null}
          {archive.collectionsCount && archive.pageNumber && archive.pageNumber >= 1 ? <>
            {language.token(MessagePageNumberToken, ["{pageNumber}"], [(archive.pageNumber + 1).toString()])}
            {archive.collectionsCount} {MessageNumberOfResults}
          </> : null}
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
