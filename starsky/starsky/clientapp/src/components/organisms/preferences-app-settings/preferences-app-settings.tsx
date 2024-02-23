import React from "react";
import PreferencesAppSettingsDesktop from "../preference-app-settings-desktop/preference-app-settings-desktop";
import PreferencesAppSettingsStorageFolder from "../preferences-app-settings-storage-folder/preferences-app-settings-storage-folder";

const PreferencesAppSettings: React.FunctionComponent = () => {
  return (
    <div className="preferences--app-settings">
      <div className="content--subheader">AppSettings</div>
      <div className="content--text">
        <PreferencesAppSettingsStorageFolder />
        <PreferencesAppSettingsDesktop />
      </div>
    </div>
  );
};

export default PreferencesAppSettings;
