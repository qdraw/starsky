import React, { useEffect } from 'react';
import Breadcrumb from '../components/breadcrumbs';
import ColorClassFilter from '../components/color-class-filter';
import DetailViewSidebar from '../components/detail-view-sidebar';
import ItemListView from '../components/item-list-view';
import RelativeLink from '../components/relative-link';
import useLocation from '../hooks/use-location';
import { IRelativeObjects } from '../interfaces/IDetailView';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { URLPath } from '../shared/url-path';

interface IArchiveProps {
  fileIndexItems: Array<IFileIndexItem>;
  relativeObjects: IRelativeObjects;
  subPath: string;
  breadcrumb: Array<string>;
  colorClassFilterList: Array<number>;
  colorClassUsage: Array<number>;
  collectionsCount: number;
  searchQuery: string;
}

function Archive(archive: IArchiveProps) {


  var history = useLocation();
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);
  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar)
  }, [history.location.search]);


  if (!archive.colorClassUsage) return (<>no colorClassUsage</>)

  return (
    <div className={!sidebar ? "archive" : "archive collapsed"}>
      {sidebar ? <DetailViewSidebar fileIndexItem={archive.fileIndexItems[0]} filePath={"fileIndexItem.filePath"}></DetailViewSidebar> : ""}

      <div className="content">
        <Breadcrumb breadcrumb={archive.breadcrumb} subPath={archive.subPath}></Breadcrumb>
        <RelativeLink relativeObjects={archive.relativeObjects}></RelativeLink>

        <ColorClassFilter itemsCount={archive.collectionsCount} subPath={archive.subPath}
          colorClassFilterList={archive.colorClassFilterList} colorClassUsage={archive.colorClassUsage}></ColorClassFilter>
        <ItemListView {...archive} colorClassUsage={archive.colorClassUsage}> </ItemListView>
      </div>
    </div>
  )
}
export default Archive;
