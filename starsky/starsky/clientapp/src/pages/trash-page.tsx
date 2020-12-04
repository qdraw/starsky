import { RouteComponentProps } from "@reach/router";
import React, { FunctionComponent } from "react";
import Preloader from "../components/atoms/preloader/preloader";
import ApplicationException from "../components/organisms/application-exception/application-exception";
import ArchiveContextWrapper from "../contexts-wrappers/archive-wrapper";
import useLocation from "../hooks/use-location";
import useSearchList from "../hooks/use-searchlist";
import { PageType } from "../interfaces/IDetailView";
import { URLPath } from "../shared/url-path";

const TrashPage: FunctionComponent<RouteComponentProps<any>> = () => {
  var history = useLocation();

  var urlObject = new URLPath().StringToIUrl(history.location.search);
  var searchList = useSearchList("!delete!", urlObject.p, true);

  if (!searchList) return <>Something went wrong</>;
  if (searchList.pageType === PageType.ApplicationException) {
    return <ApplicationException></ApplicationException>;
  }
  if (!searchList.archive) return <>Something went wrong</>;
  if (searchList.pageType === PageType.Loading)
    return (
      <Preloader
        isTransition={false}
        isOverlay={true}
        isDetailMenu={false}
      ></Preloader>
    );

  return <ArchiveContextWrapper {...searchList.archive} />;
};

export default TrashPage;
