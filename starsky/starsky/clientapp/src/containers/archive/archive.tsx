import React, { useEffect } from "react";
import ArchivePagination from "../../components/molecules/archive-pagination/archive-pagination";
import Breadcrumb from "../../components/molecules/breadcrumbs/breadcrumbs";
import ColorClassFilter from "../../components/molecules/color-class-filter/color-class-filter";
import ItemListView from "../../components/molecules/item-list-view/item-list-view";
import ArchiveSidebar from "../../components/organisms/archive-sidebar/archive-sidebar";
import MenuArchive from "../../components/organisms/menu-archive/menu-archive";
import useLocation from "../../hooks/use-location/use-location";
import { IArchiveProps } from "../../interfaces/IArchiveProps";
import { URLPath } from "../../shared/url/url-path";

function Archive(archive: Readonly<IArchiveProps>) {
  const history = useLocation();

  const iconList = !new URLPath().StringToIUrl(history.location.search).list;

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
      <div className={!sidebar ? "archive" : "archive collapsed"}>
        <ArchiveSidebar {...archive} />

        <div className="content">
          <Breadcrumb breadcrumb={archive.breadcrumb} subPath={archive.subPath} />
          <ArchivePagination relativeObjects={archive.relativeObjects} />

          <ColorClassFilter
            itemsCount={archive.collectionsCount}
            subPath={archive.subPath}
            colorClassActiveList={archive.colorClassActiveList}
            colorClassUsage={archive.colorClassUsage}
            sticky={true}
          />
          <ItemListView iconList={iconList} {...archive}></ItemListView>
        </div>
      </div>
    </>
  );
}
export default Archive;
