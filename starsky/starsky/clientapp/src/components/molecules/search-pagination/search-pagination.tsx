import { Link } from '@reach/router';
import React, { memo, useEffect } from "react";
import useGlobalSettings from '../../../hooks/use-global-settings';
import useLocation from '../../../hooks/use-location';
import { Language } from '../../../shared/language';
import { URLPath } from '../../../shared/url-path';

export interface IRelativeLink {
  lastPageNumber?: number;
}

/**
 * Next prev for search pages
 */
const SearchPagination: React.FunctionComponent<IRelativeLink> = memo((props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessagePrevious = language.text("Vorige", "Previous");
  const MessageNext = language.text("Volgende", "Next");

  // used for reading current location
  var history = useLocation();

  const [lastPageNumber, setLastPageNumber] = React.useState(-1);
  const [urlObject, setUrlObject] = React.useState(new URLPath().StringToIUrl(history.location.search));

  useEffect(() => {
    setLastPageNumber(props.lastPageNumber ? props.lastPageNumber : -1);
    setUrlObject(new URLPath().StringToIUrl(history.location.search));
  }, [props, history.location.search]);

  function prev(): JSX.Element {
    if (!urlObject || !lastPageNumber) return <></>;
    urlObject.p = urlObject.p ? urlObject.p : 0;
    var prevObject = { ...urlObject };
    prevObject.p = prevObject.p ? prevObject.p - 1 : 1;
    if (!urlObject.p || urlObject.p < 0 || lastPageNumber < urlObject.p) return <></>;
    return <Link onClick={() => window.scrollTo(0, 0)} className="prev" to={new URLPath().IUrlToString(prevObject)}> {MessagePrevious}</ Link>;
  }

  function next(): JSX.Element {
    if (!urlObject || !lastPageNumber) return <></>;
    urlObject.p = urlObject.p ? urlObject.p : 0;
    var nextObject = { ...urlObject };
    nextObject.p = nextObject.p ? nextObject.p + 1 : 1;
    // if(urlObject.p) also means 0
    if (urlObject.p === undefined || urlObject.p < 0 || lastPageNumber <= urlObject.p) return <></>; // undefined=0
    return <Link onClick={() => window.scrollTo(0, 0)} className="next" to={new URLPath().IUrlToString(nextObject)}> {MessageNext}</ Link>;
  }

  return (<>
    <div className="relativelink">
      <h4 className="nextprev">
        {prev()}
        {next()}
      </h4>
    </div>
  </>);


});
export default SearchPagination