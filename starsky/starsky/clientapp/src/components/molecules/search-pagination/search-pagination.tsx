import { Link } from "@reach/router";
import React, { memo, useEffect } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location";
import { IUrl } from "../../../interfaces/IUrl";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";

export interface IRelativeLink {
  lastPageNumber?: number;
}

/**
 * Next prev for search pages
 */
const SearchPagination: React.FunctionComponent<IRelativeLink> = memo(
  (props) => {
    // content
    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessagePrevious = language.text("Vorige", "Previous");
    const MessageNext = language.text("Volgende", "Next");

    // used for reading current location
    const history = useLocation();

    const [lastPageNumber, setLastPageNumber] = React.useState(-1);
    const [urlObject, setUrlObject] = React.useState(
      new URLPath().StringToIUrl(history.location.search)
    );

    useEffect(() => {
      setLastPageNumber(props.lastPageNumber ? props.lastPageNumber : -1);
      let urlObjectLocal = new URLPath().StringToIUrl(history.location.search);
      urlObjectLocal = resetSelect(urlObjectLocal);
      setUrlObject(urlObjectLocal);
    }, [props, history.location.search]);

    function prev(): React.JSX.Element {
      if (!urlObject || !lastPageNumber) return <></>;
      urlObject.p = urlObject.p ? urlObject.p : 0;
      const prevObject = { ...urlObject };
      prevObject.p = prevObject.p ? prevObject.p - 1 : 1;
      if (!urlObject.p || urlObject.p < 0 || lastPageNumber < urlObject.p)
        return <></>;
      return (
        <Link
          onClick={() => window.scrollTo(0, 0)}
          className="prev"
          data-test="search-pagination-prev"
          to={new URLPath().IUrlToString(prevObject)}
        >
          {" "}
          {MessagePrevious}
        </Link>
      );
    }

    function next(): React.JSX.Element {
      if (!urlObject || !lastPageNumber) return <></>;
      urlObject.p = urlObject.p ? urlObject.p : 0;
      const nextObject = { ...urlObject };
      nextObject.p = nextObject.p ? nextObject.p + 1 : 1;
      // if(urlObject.p) also means 0
      if (
        urlObject.p === undefined ||
        urlObject.p < 0 ||
        lastPageNumber <= urlObject.p
      )
        return <></>; // undefined=0
      return (
        <Link
          onClick={() => window.scrollTo(0, 0)}
          className="next"
          data-test="search-pagination-next"
          to={new URLPath().IUrlToString(nextObject)}
        >
          {" "}
          {MessageNext}
        </Link>
      );
    }

    /**
     * when in select mode and navigate next to the select mode is still on but there are no items selected
     */
    function resetSelect(urlObjectLocal: IUrl): IUrl {
      if (!urlObjectLocal.select || urlObjectLocal.select?.length === 0) {
        return urlObjectLocal;
      }
      urlObjectLocal.select = [];
      return urlObjectLocal;
    }

    return (
      <>
        <div className="relativelink" data-test="search-pagination">
          <h4 className="nextprev">
            {prev()}
            {next()}
          </h4>
        </div>
      </>
    );
  }
);
export default SearchPagination;
