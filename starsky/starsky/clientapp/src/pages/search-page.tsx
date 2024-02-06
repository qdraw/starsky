import { FunctionComponent } from "react";
import Preloader from "../components/atoms/preloader/preloader";
import ApplicationException from "../components/organisms/application-exception/application-exception";
import ArchiveContextWrapper from "../contexts-wrappers/archive-wrapper";
import useLocation from "../hooks/use-location/use-location";
import useSearchList from "../hooks/use-searchlist";
import { PageType } from "../interfaces/IDetailView";
import { URLPath } from "../shared/url-path";

export const SearchPage: FunctionComponent = () => {
  const history = useLocation();

  const urlObject = new URLPath().StringToIUrl(history.location.search);
  const searchList = useSearchList(urlObject.t, urlObject.p, true);

  if (!searchList) return <>Something went wrong</>;
  if (searchList.pageType === PageType.ApplicationException) {
    return <ApplicationException></ApplicationException>;
  }
  if (!searchList.archive) return <>Something went wrong</>;

  return (
    <>
      {searchList.pageType === PageType.Loading ? (
        <Preloader isOverlay={true} isWhite={false}></Preloader>
      ) : null}
      <ArchiveContextWrapper {...searchList.archive} />
    </>
  );
};
