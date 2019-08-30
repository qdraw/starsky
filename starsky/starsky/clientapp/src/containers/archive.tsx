import React, { useEffect } from 'react';
import ArchiveSidebar from '../components/archive-sidebar';
import Breadcrumb from '../components/breadcrumbs';
import ColorClassFilter from '../components/color-class-filter';
import ItemListView from '../components/item-list-view';
import RelativeLink from '../components/relative-link';
import useLocation from '../hooks/use-location';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { URLPath } from '../shared/url-path';


function Archive(archive: IArchiveProps) {

  var history = useLocation();

  // The sidebar
  const [sidebar, setSidebar] = React.useState(new URLPath().StringToIUrl(history.location.search).sidebar);
  useEffect(() => {
    setSidebar(new URLPath().StringToIUrl(history.location.search).sidebar)
  }, [history.location.search]);

  // // To update the list of items
  // const [archiveList, setArchiveList] = React.useState(archive);
  // let { state } = React.useContext(ArchiveContext);
  // useEffect(() => {
  //   setArchiveList(state);
  //   console.log('u');
  // }, [archive]);

  // // var archiveList = archive;
  // // // console.log(archive);

  if (!archive) return (<>no archive</>)
  if (!archive.colorClassUsage) return (<>no colorClassUsage</>)

  return (
    <div className={!sidebar ? "archive" : "archive collapsed"}>
      {sidebar ? <ArchiveSidebar {...archive}></ArchiveSidebar> : ""}

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
