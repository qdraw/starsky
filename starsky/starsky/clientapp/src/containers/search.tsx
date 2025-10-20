import React, { useEffect } from "react";
import ItemListView from "../components/molecules/item-list-view/item-list-view";
import MenuSearchBar from "../components/molecules/menu-inline-search/menu-inline-search";
import SearchPagination from "../components/molecules/search-pagination/search-pagination";
import ArchiveSidebar from "../components/organisms/archive-sidebar/archive-sidebar";
import useGlobalSettings from "../hooks/use-global-settings";
import useLocation from "../hooks/use-location/use-location";
import { IArchiveProps } from "../interfaces/IArchiveProps";
import localization from "../localization/localization.json";
import { Language } from "../shared/language";
import { URLPath } from "../shared/url/url-path";
import MenuMenuSearchContainer from "./menu-search-container/menu-search-container";

function Search(archive: Readonly<IArchiveProps>) {
  // Content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageNumberOfResults = language.key(localization.MessageNumberOfResults);
  const MessageNoResult = language.key(localization.MessageNoResult);
  const MessageTryOtherQuery = language.key(localization.MessageTryOtherQuery);
  const MessagePageNumberToken = language.key(localization.MessagePageNumberToken); // space at end

  const history = useLocation();

  // The sidebar
  const [sidebar, setSidebar] = React.useState(
    new URLPath().StringToIUrl(history.location.search).sidebar
  );

  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar);
  }, [history.location.search]);

  const [query, setQuery] = React.useState(new URLPath().StringToIUrl(history.location.search).t);
  useEffect(() => {
    setQuery(new URLPath().StringToIUrl(history.location.search).t);
  }, [history.location.search]);

  if (!archive) return <>(Search) no archive</>;
  if (!archive.colorClassUsage) return <>(Search) no colorClassUsage</>;

  return (
    <>
      <MenuMenuSearchContainer />
      <div className={sidebar ? "archive collapsed" : "archive"}>
        <ArchiveSidebar {...archive} />
        <div className="content">
          <div className="search-header">
            <MenuSearchBar defaultText={query} />
          </div>
          <div className="content--header" data-test="search-content-header">
            {archive.collectionsCount ? null : MessageNoResult}
            {archive.collectionsCount && archive.pageNumber === 0 ? (
              <>
                {archive.collectionsCount} {MessageNumberOfResults}
              </>
            ) : null}
            {archive.collectionsCount && archive.pageNumber && archive.pageNumber >= 1 ? (
              <>
                {language.token(
                  MessagePageNumberToken,
                  ["{pageNumber}"],
                  [(archive.pageNumber + 1).toString()]
                )}
                {archive.collectionsCount} {MessageNumberOfResults}
              </>
            ) : null}
          </div>
          <SearchPagination {...archive} />
          {archive.collectionsCount >= 1 ? (
            <ItemListView iconList={true} {...archive} colorClassUsage={archive.colorClassUsage} />
          ) : null}
          {archive.collectionsCount === 0 ? (
            <div className="folder">
              <div className="warning-box">{MessageTryOtherQuery}</div>
            </div>
          ) : null}
          {archive.lastPageNumber === 0 ? null : <SearchPagination {...archive} />}
        </div>
      </div>
    </>
  );
}
export default Search;
