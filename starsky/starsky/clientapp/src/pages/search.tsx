
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import Menu from '../containers/menu';



interface ISearchPageProps {
}

const Search: FunctionComponent<RouteComponentProps<ISearchPageProps>> = (props) => {
  var sidebar = false;
  return (<div>
    <Menu detailView={undefined} parent={"parent"} isDetailMenu={false}></Menu>

    {props.path}
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


export default Search;
