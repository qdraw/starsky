import { RouteComponentProps } from "@reach/router";
import React, { FunctionComponent } from "react";
import Preloader from "../components/atoms/preloader/preloader";
import ApplicationException from "../components/organisms/application-exception/application-exception";
import ArchiveContextWrapper from "../contexts-wrappers/archive-wrapper";
import useLocation from "../hooks/use-location";
import useSearchList from "../hooks/use-searchlist";
import { PageType } from "../interfaces/IDetailView";
import { URLPath } from "../shared/url-path";

const SearchPage: FunctionComponent<RouteComponentProps<any>> = () => {
  var history = useLocation();

  var urlObject = new URLPath().StringToIUrl(history.location.search);
  var searchList = useSearchList(urlObject.t, urlObject.p, true);

  if (!searchList) return <>Something went wrong</>;
  if (searchList.pageType === PageType.ApplicationException) {
    return <ApplicationException></ApplicationException>;
  }
  if (!searchList.archive) return <>Something went wrong</>;

  return (
    <>
      {searchList.pageType === PageType.Loading ? (
        <Preloader isOverlay={true} isDetailMenu={false}></Preloader>
      ) : null}
      <ArchiveContextWrapper {...searchList.archive} />
    </>
  );
};

export default SearchPage;
