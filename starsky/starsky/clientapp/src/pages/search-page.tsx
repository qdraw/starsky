
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MenuSearch from '../components/menu-search';
import ArchiveContextWrapper from '../containers/archive-wrapper';
import useLocation from '../hooks/use-location';
import useSearchList from '../hooks/use-searchlist';
import { URLPath } from '../shared/url-path';



interface ISearchPageProps {
}

const SearchPage: FunctionComponent<RouteComponentProps<ISearchPageProps>> = (props) => {
  var history = useLocation();

  var urlObject = new URLPath().StringToIUrl(history.location.search);
  var archive = useSearchList(urlObject.t, urlObject.p);

  if (!archive) return (<>Something went wrong</>)
  if (!archive.archive) return (<>Something went wrong</>)

  return (<div>
    <MenuSearch detailView={undefined} parent={"parent"} isDetailMenu={false}></MenuSearch>
    <ArchiveContextWrapper {...archive.archive} />
  </div>)
}


export default SearchPage;
