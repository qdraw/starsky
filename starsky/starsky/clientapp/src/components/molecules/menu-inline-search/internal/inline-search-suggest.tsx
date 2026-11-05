import { useEffect, useState } from "react";
import useGlobalSettings from "../../../../hooks/use-global-settings";
import useLocation from "../../../../hooks/use-location/use-location";
import { IConnectionDefault } from "../../../../interfaces/IConnectionDefault";
import { IEnvFeatures } from "../../../../interfaces/IEnvFeatures";
import localization from "../../../../localization/localization.json";
import { useTenants } from "../../../../hooks/use-tenants/use-tenants";
import { Language } from "../../../../shared/language";
import { TenantFromLocation, UrlQuery } from "../../../../shared/url/url-query";
import Navigate from "./navigate";

export interface IInlineSearchSuggestProps {
  suggest: string[];
  setFormFocus: React.Dispatch<React.SetStateAction<boolean>>;
  inputFormControlReference: React.RefObject<HTMLInputElement | null>;
  featuresResult: IConnectionDefault;
  defaultText?: string;
  callback?: (query: string) => void;
}

const InlineSearchSuggest: React.FunctionComponent<IInlineSearchSuggestProps> = (props) => {
  const history = useLocation();
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const dataFeatures = props.featuresResult?.data as IEnvFeatures | undefined;

  // only users with more than one tenant see this: the other tenants to switch to
  const tenants = useTenants();
  const otherTenants =
    tenants && tenants.tenants.length > 1
      ? tenants.tenants.filter((tenant) => tenant.tenantId !== TenantFromLocation())
      : [];
  const MessageSwitchTenant = language.key(localization.MessageSwitchTenant);

  useEffect(() => {
    if (dataFeatures?.systemTrashEnabled || dataFeatures?.useLocalDesktop) {
      let newMenu = [...defaultMenu];
      if (dataFeatures?.systemTrashEnabled) {
        newMenu = newMenu.filter((item) => item.key !== "trash");
      }
      if (dataFeatures?.useLocalDesktop) {
        newMenu = newMenu.filter((item) => item.key !== "logout");
      }
      setDefaultMenu([...newMenu]);
    }
    // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
  }, [props.featuresResult, dataFeatures?.systemTrashEnabled]);

  const [defaultMenu, setDefaultMenu] = useState([
    {
      name: language.key(localization.MessageHome),
      url: new UrlQuery().UrlHomePage(),
      key: "home"
    },
    {
      name: language.key(localization.MessagePhotosOfThisWeek),
      // search?t=-Datetime>7
      url: new UrlQuery().UrlSearchPage("-Datetime%3E7%20"),
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
      // command + shift + k -> see GlobalShortcuts
    },
    {
      name: language.key(localization.MessageLogout),
      url: new UrlQuery().UrlLoginPage(),
      key: "logout"
    }
  ]);

  return (
    <>
      {props.suggest?.length === 0
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
      {props.suggest?.length === 0
        ? otherTenants.map((tenant) => (
            <li
              className="menu-item menu-item--default"
              key={`tenant-${tenant.tenantId}`}
              data-test={`default-menu-item-tenant-${tenant.tenantId}`}
            >
              {/* a full page load: nothing of the other tenant may stay in the page */}
              <a href={new UrlQuery().UrlTenantHomePage(tenant.tenantId)}>
                {MessageSwitchTenant} {tenant.displayName}
              </a>
            </li>
          ))
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
              type="button"
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
