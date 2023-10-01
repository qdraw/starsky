import { useEffect, useState } from "react";
import useGlobalSettings from "../../../../hooks/use-global-settings";
import useLocation from "../../../../hooks/use-location/use-location";
import { IConnectionDefault } from "../../../../interfaces/IConnectionDefault";
import { IEnvFeatures } from "../../../../interfaces/IEnvFeatures";
import localization from "../../../../localization/localization.json";
import { Language } from "../../../../shared/language";
import { UrlQuery } from "../../../../shared/url-query";
import Navigate from "./navigate";

export interface IInlineSearchSuggestProps {
  suggest: string[];
  setFormFocus: React.Dispatch<React.SetStateAction<boolean>>;
  inputFormControlReference: React.RefObject<HTMLInputElement>;
  featuresResult: IConnectionDefault;
  defaultText?: string;
  callback?: (query: string) => void;
}

const InlineSearchSuggest: React.FunctionComponent<
  IInlineSearchSuggestProps
> = (props) => {
  const history = useLocation();
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  useEffect(() => {
    const dataFeatures = props.featuresResult?.data as IEnvFeatures | undefined;
    if (dataFeatures?.systemTrashEnabled || dataFeatures?.useLocalDesktopUi) {
      let newMenu = [...defaultMenu];
      if (dataFeatures?.systemTrashEnabled) {
        newMenu = newMenu.filter((item) => item.key !== "trash");
      }
      if (dataFeatures?.useLocalDesktopUi) {
        newMenu = newMenu.filter((item) => item.key !== "logout");
      }
      setDefaultMenu([...newMenu]);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.featuresResult, props.featuresResult?.data?.systemTrashEnabled]);

  const [defaultMenu, setDefaultMenu] = useState([
    {
      name: language.key(localization.MessageHome),
      url: new UrlQuery().UrlHomePage(),
      key: "home"
    },
    {
      name: language.key(localization.MessagePhotosOfThisWeek),
      // search?t=-Datetime>7 -ImageFormat-"xmp,tiff"
      url: new UrlQuery().UrlSearchPage(
        "-Datetime%3E7%20-ImageFormat-%22xmp,tiff%22"
      ),
      key: "photos-of-this-week"
    },
    {
      name: language.key(localization.MessageTrash),
      url: new UrlQuery().UrlTrashPage(),
      key: "trash"
    },
    {
      name: language.key(localization.MessageImport),
      url: new UrlQuery().UrlImportPage(),
      key: "import"
    },
    {
      name: language.key(localization.MessagePreferences),
      url: new UrlQuery().UrlPreferencesPage(),
      key: "preferences"
    },
    {
      name: language.key(localization.MessageLogout),
      url: new UrlQuery().UrlLoginPage(),
      key: "logout"
    }
  ]);

  return (
    <>
      {props.suggest && props.suggest.length === 0
        ? defaultMenu.map((value) => {
            return (
              <li
                className="menu-item menu-item--default"
                key={value.name}
                data-test={`default-menu-item-${value.key}`}
              >
                <a href={value.url}>{value.name}</a>{" "}
              </li>
            );
          })
        : null}
      {props.suggest?.map((query, index) =>
        index <= 8 ? (
          <li
            key={query}
            data-key={query}
            data-test={"menu-inline-search-suggest-" + query}
            className="menu-item menu-item--results"
          >
            <button
              onClick={() =>
                Navigate(
                  history,
                  props.setFormFocus,
                  props.inputFormControlReference,
                  query,
                  props.callback
                )
              }
              className="search-icon"
              data-test="menu-inline-search-search-icon"
            >
              {query}
            </button>
          </li>
        ) : null
      )}
    </>
  );
};

export default InlineSearchSuggest;
