
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MenuSearch from '../components/menu-search';
import MenuSearchBar from '../components/menu.searchbar';
import Preloader from '../components/preloader';
import ArchiveContextWrapper from '../contexts-wrappers/archive-wrapper';
import useLocation from '../hooks/use-location';
import useSearchList from '../hooks/use-searchlist';
import { PageType } from '../interfaces/IDetailView';
import { URLPath } from '../shared/url-path';



interface ISearchPageProps {
}

const SearchPage: FunctionComponent<RouteComponentProps<ISearchPageProps>> = (props) => {
  var history = useLocation();

  var urlObject = new URLPath().StringToIUrl(history.location.search);
  var searchList = useSearchList(urlObject.t, urlObject.p);

  if (!searchList) return (<>Something went wrong</>)
  if (!searchList.archive) return (<>Something went wrong</>)
  if (searchList.pageType === PageType.Loading) return (<Preloader isOverlay={true} isDetailMenu={false}></Preloader>)

  return (<div>
    <MenuSearch></MenuSearch>

    {searchList.pageType === PageType.NotFound ? <div className="content">
      <div className="search-header">
        <MenuSearchBar defaultText=""></MenuSearchBar>
      </div>
      <div className="content--header">Geen resultaat</div></div> : null}

    <ArchiveContextWrapper {...searchList.archive} />
  </div>)
}


export default SearchPage;
