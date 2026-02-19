import { useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import MenuDefault from "../../components/organisms/menu-default/menu-default";
import PreferencesAppSettings from "../../components/organisms/preferences-app-settings/preferences-app-settings";
import PreferencesCloudImport from "../../components/organisms/preferences-cloud-import/preferences-cloud-import";
import PreferencesPassword from "../../components/organisms/preferences-password/preferences-password";
import PreferencesUsername from "../../components/organisms/preferences-username/preferences-username";
import useGlobalSettings from "../../hooks/use-global-settings";
import localization from "../../localization/localization.json";
import { Language } from "../../shared/language";

type PreferencesTab = "username" | "password" | "app" | "cloud";

const tabValues: Set<PreferencesTab> = new Set(["username", "password", "app", "cloud"]);

const isPreferencesTab = (value: string | null): value is PreferencesTab => {
  if (!value) {
    return false;
  }
  return tabValues.has(value as PreferencesTab);
};

export const Preferences: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const messagePreferences = language.key(localization.MessagePreferences);
  const messageUsername = language.key(localization.MessageUsername);
  const messagePassword = language.key(localization.MessagePassword);
  const messageAppSettings = language.key(localization.MessageAppSettings);
  const messageCloudImports = language.key(localization.MessageCloudImports);

  const [searchParams, setSearchParams] = useSearchParams();

  const currentTab = searchParams.get("tab");
  const activeTab: PreferencesTab = isPreferencesTab(currentTab) ? currentTab : "username";

  useEffect(() => {
    if (currentTab === activeTab) {
      return;
    }
    const nextSearchParams = new URLSearchParams(searchParams);
    nextSearchParams.set("tab", activeTab);
    setSearchParams(nextSearchParams, { replace: true });
  }, [activeTab, currentTab, searchParams, setSearchParams]);

  const tabs: { id: PreferencesTab; label: string }[] = [
    { id: "username", label: messageUsername },
    { id: "password", label: messagePassword },
    { id: "app", label: messageAppSettings },
    { id: "cloud", label: messageCloudImports }
  ];

  const onChangeTab = (tab: PreferencesTab) => {
    if (tab === activeTab) {
      return;
    }
    const nextSearchParams = new URLSearchParams(searchParams);
    nextSearchParams.set("tab", tab);
    setSearchParams(nextSearchParams);
  };

  return (
    <>
      <MenuDefault isEnabled={true} />
      <div className="content--header">{messagePreferences}</div>

      <div className="preferences-tabs" role="tablist" aria-label="Preferences sections">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            type="button"
            role="tab"
            data-test={`preferences-tab-${tab.id}`}
            aria-selected={activeTab === tab.id}
            className={
              activeTab === tab.id
                ? "preferences-tabs__item preferences-tabs__item--active"
                : "preferences-tabs__item"
            }
            onClick={() => onChangeTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {activeTab === "username" && <PreferencesUsername />}
      {activeTab === "password" && <PreferencesPassword />}
      {activeTab === "app" && <PreferencesAppSettings />}
      {activeTab === "cloud" && <PreferencesCloudImport />}
    </>
  );
};
