import { useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import MenuDefault from "../../components/organisms/menu-default/menu-default";
import PreferencesAppSettings from "../../components/organisms/preferences-app-settings/preferences-app-settings";
import PreferencesPassword from "../../components/organisms/preferences-password/preferences-password";
import PreferencesUsername from "../../components/organisms/preferences-username/preferences-username";
import useGlobalSettings from "../../hooks/use-global-settings";
import localization from "../../localization/localization.json";
import { Language } from "../../shared/language";

type PreferencesTab = "username" | "password" | "app";

const tabValues: PreferencesTab[] = ["username", "password", "app"];

const isPreferencesTab = (value: string | null): value is PreferencesTab => {
  if (!value) {
    return false;
  }
  return tabValues.includes(value as PreferencesTab);
};

export const Preferences: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessagePreferences = language.key(localization.MessagePreferences);
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
    { id: "username", label: "Username" },
    { id: "password", label: "Password" },
    { id: "app", label: "AppSettings" }
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
      <div className="content--header">{MessagePreferences}</div>

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
    </>
  );
};
