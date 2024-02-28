import useGlobalSettings from "../../../hooks/use-global-settings";
import { IUseLocation } from "../../../hooks/use-location/interfaces/IUseLocation";
import { IDetailView } from "../../../interfaces/IDetailView";
import { INavigateState } from "../../../interfaces/INavigateState";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import Link from "../../atoms/link/link";

type IsSearchQueryMenuSearchItemProps = {
  isSearchQuery: boolean;
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  state: IDetailView;
  history: IUseLocation;
};

const IsSearchQueryMenuSearchItem: React.FunctionComponent<IsSearchQueryMenuSearchItemProps> = ({
  isSearchQuery,
  state,
  history,
  setIsLoading
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  if (isSearchQuery) {
    const query = new URLPath().StringToIUrl(history.location.search).t;
    return (
      <Link
        data-test="menu-detail-view-close"
        className="item item--first item--search"
        onClick={(event) => {
          // Command (mac) or ctrl click means open new window
          // event.button = is only trigged in safari
          if (event.metaKey || event.ctrlKey || event.button === 1) return;

          setIsLoading(true);
        }}
        state={{ filePath: state.fileIndexItem.filePath } as INavigateState}
        to={new UrlQuery().HashSearchPage(history.location.search)}
      >
        {query === "!delete!" ? language.key(localization.MessageTrash) : query}
      </Link>
    );
  }
  return <></>;
};

export default IsSearchQueryMenuSearchItem;
