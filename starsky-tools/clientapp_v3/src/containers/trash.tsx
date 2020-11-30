import React, { useEffect } from 'react';
import ItemListView from '../components/molecules/item-list-view/item-list-view';
import SearchPagination from '../components/molecules/search-pagination/search-pagination';
import ArchiveSidebar from '../components/organisms/archive-sidebar/archive-sidebar';
import MenuTrash from '../components/organisms/menu-trash/menu-trash';
import { ArchiveContext, defaultStateFallback } from '../contexts/archive-context';
import useGlobalSettings from '../hooks/use-global-settings';
import useLocation from '../hooks/use-location';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { Language } from '../shared/language';
import { URLPath } from '../shared/url-path';

function Trash(archive: IArchiveProps) {

  // Content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageEmptyTrash = language.text("Er staat niets in de prullenmand", "There is nothing in the trash");
  const MessageNumberOfResults = language.text("resultaten", "results");
  const MessageNoResult = language.text("Geen resultaat", "No result");

  var history = useLocation();

  // The sidebar
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);

  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar)
  }, [history.location.search]);

  // to dynamic update the number of trashed items
  let { state } = React.useContext(ArchiveContext);
  state = defaultStateFallback(state);

  const [collectionsCount, setCollectionsCount] = React.useState(state.collectionsCount);
  useEffect(() => {
    setCollectionsCount(state.collectionsCount);
  }, [state.collectionsCount]);

  if (!archive) return (<>(Search) = no archive</>);
  if (!archive.colorClassUsage) return (<>(Search) = no colorClassUsage</>);

  return (
    <>
      <MenuTrash />
      <div className={!sidebar ? "archive" : "archive collapsed"}>
        {sidebar ? <ArchiveSidebar {...archive} /> : ""}

        <div className="content">
          <div className="content--header">{collectionsCount !== 0 ?
            <>{collectionsCount} {MessageNumberOfResults}</>
            : MessageNoResult}
          </div>
          <SearchPagination {...archive} />
          {collectionsCount >= 1 ? <ItemListView {...archive} colorClassUsage={archive.colorClassUsage}> </ItemListView> : null}
          {collectionsCount === 0 ? <div className="folder">
            <div className="warning-box"> {MessageEmptyTrash}</div>
          </div> : null}
          {archive.fileIndexItems.length >= 20 ? <SearchPagination {...archive} /> : null}
        </div>
      </div>
    </>
  )
}
export default Trash;
