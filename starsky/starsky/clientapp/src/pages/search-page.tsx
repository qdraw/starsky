
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MenuSearch from '../components/menu-search';
import ArchiveContextWrapper from '../containers/archive-wrapper';
import useLocation from '../hooks/use-location';
import useSearchList from '../hooks/use-searchlist';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { URLPath } from '../shared/url-path';



interface ISearchPageProps {
}

const SearchPage: FunctionComponent<RouteComponentProps<ISearchPageProps>> = (props) => {
  var sidebar = false;
  var history = useLocation();

  var urlObject = new URLPath().StringToIUrl(history.location.search);
  var archive = useSearchList(urlObject.t, urlObject.p);

  if (!archive) return (<>Something went wrong</>)
  if (!archive.archive) return (<>Something went wrong</>)

  var archiveProps = {} as IArchiveProps;
  return (<div>
    <MenuSearch detailView={undefined} parent={"parent"} isDetailMenu={false}></MenuSearch>
    <ArchiveContextWrapper {...archive.archive} />

    {/* <div className={!sidebar ? "archive" : "archive collapsed"}>
      {sidebar ? <ArchiveSidebar {...archive}></ArchiveSidebar> : ""}

      <div className="content">
        <Breadcrumb breadcrumb={archive.breadcrumb} subPath={archive.subPath}></Breadcrumb>
        <RelativeLink relativeObjects={archive.relativeObjects}></RelativeLink>

        <ColorClassFilter itemsCount={archive.collectionsCount} subPath={archive.subPath}
          colorClassFilterList={archive.colorClassFilterList} colorClassUsage={archive.colorClassUsage}></ColorClassFilter>
        <ItemListView {...archive} colorClassUsage={archive.colorClassUsage}> </ItemListView>
      </div>
    </div> */}
  </div>)
}


export default SearchPage;
