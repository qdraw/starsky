import React, { useEffect } from "react";
import ArchivePagination from "../../components/molecules/archive-pagination/archive-pagination";
import Breadcrumb from "../../components/molecules/breadcrumbs/breadcrumbs";
import ColorClassFilter from "../../components/molecules/color-class-filter/color-class-filter";
import ItemListView from "../../components/molecules/item-list-view/item-list-view";
import SharedStructuredFilter from "../../components/molecules/shared-structured-filter/shared-structured-filter";
import ArchiveSidebar from "../../components/organisms/archive-sidebar/archive-sidebar";
import MenuArchive from "../../components/organisms/menu-archive/menu-archive";
import useGlobalSettings from "../../hooks/use-global-settings";
import useLocation from "../../hooks/use-location/use-location";
import { IArchiveProps } from "../../interfaces/IArchiveProps";
import localization from "../../localization/localization.json";
import { Language } from "../../shared/language";
import { UrlQuery } from "../../shared/url/url-query";
import { URLPath } from "../../shared/url/url-path";

function Archive(archive: Readonly<IArchiveProps>) {
  const history = useLocation();
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageItemsOutsideFilter = language.key(localization.MessageItemsOutsideFilter);

  const urlObject = new URLPath().StringToIUrl(history.location.search);
  const iconList = !urlObject.list;

  // The sidebar
  const [sidebar, setSidebar] = React.useState(
    new URLPath().StringToIUrl(history.location.search).sidebar
  );
  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar);
  }, [history.location.search]);

  if (archive && (!archive.colorClassUsage || !archive.colorClassActiveList))
    return <>(Archive) = no colorClassLists</>;

  return (
    <>
      <MenuArchive />
      <div className={sidebar ? "archive collapsed" : "archive"}>
        <ArchiveSidebar {...archive} />

        <div className="content">
          <Breadcrumb breadcrumb={archive.breadcrumb} subPath={archive.subPath} />
          <ArchivePagination relativeObjects={archive.relativeObjects} />

          <SharedStructuredFilter
            urlObject={urlObject}
            onChange={(nextUrl) => {
              history.navigate(new URLPath().IUrlToString(nextUrl), { replace: true });
            }}
          />

          <ColorClassFilter
            itemsCount={archive.collectionsCount}
            subPath={archive.subPath}
            colorClassActiveList={archive.colorClassActiveList}
            colorClassUsage={archive.colorClassUsage}
            sticky={true}
          />
          {archive.collectionsCount === 0 && new UrlQuery().HasStructuredFilters(urlObject) ? (
            <div className="warning-box warning-box--left" data-test="archive-structured-filter-warning">
              {MessageItemsOutsideFilter}
            </div>
          ) : null}
          <ItemListView iconList={iconList} {...archive}></ItemListView>
        </div>
      </div>
    </>
  );
}
export default Archive;
