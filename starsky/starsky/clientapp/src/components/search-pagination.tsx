import { Link } from '@reach/router';
import React, { memo, useEffect } from "react";
import useLocation from '../hooks/use-location';
import { URLPath } from '../shared/url-path';

export interface IRelativeLink {
  lastPageNumber?: number;
}

const SearchPagination: React.FunctionComponent<IRelativeLink> = memo((props) => {

  // used for reading current location
  var history = useLocation();

  const [lastPageNumber, setLastPageNumber] = React.useState(-1);
  const [urlObject, setUrlObject] = React.useState(new URLPath().StringToIUrl(history.location.search));

  useEffect(() => {
    setLastPageNumber(props.lastPageNumber ? props.lastPageNumber : -1);
    setUrlObject(new URLPath().StringToIUrl(history.location.search))
  }, [props, history.location.search]);

  function prev(): JSX.Element {
    if (!urlObject || !lastPageNumber) return <></>;
    urlObject.p = urlObject.p ? urlObject.p : 0;
    var prevObject = { ...urlObject };
    prevObject.p = prevObject.p ? prevObject.p - 1 : 1;
    if (!urlObject.p || urlObject.p < 0 || lastPageNumber < urlObject.p) return <></>;
    return <Link className="prev" to={new URLPath().IUrlToString(prevObject)}> Vorige</ Link>;
  }

  function next(): JSX.Element {
    if (!urlObject || !lastPageNumber) return <></>;
    urlObject.p = urlObject.p ? urlObject.p : 0;
    var nextObject = { ...urlObject };
    nextObject.p = nextObject.p ? nextObject.p + 1 : 1;
    // if(urlObject.p) also means 0
    if (urlObject.p === undefined || urlObject.p < 0 || lastPageNumber <= urlObject.p) return <></>; // undefined=0
    return <Link className="next" to={new URLPath().IUrlToString(nextObject)}> Volgende</ Link>;
  }

  return (<div className="relativelink"><h4 className="nextprev">
    {prev()}
    {next()}
  </h4></div>);


});
export default SearchPagination