import React from "react";
import PreferencesAppSettingsDesktop from "../preference-app-settings-desktop/preference-app-settings-desktop";
import PreferencesAppSettingsStorageFolder from "../preferences-app-settings-storage-folder/preferences-app-settings-storage-folder";
import PreferencesAppSettingsStorageFolderMappings from "../preferences-app-settings-storage-folder-mappings/preferences-app-settings-storage-folder-mappings";

const PreferencesAppSettings: React.FunctionComponent = () => {
  return (
    <div className="preferences--app-settings">
      <div className="content--subheader">AppSettings</div>
      <div className="content--text">
        <PreferencesAppSettingsStorageFolder />
        <PreferencesAppSettingsStorageFolderMappings />
        <PreferencesAppSettingsDesktop />
      </div>
    </div>
  );
};

export default PreferencesAppSettings;
