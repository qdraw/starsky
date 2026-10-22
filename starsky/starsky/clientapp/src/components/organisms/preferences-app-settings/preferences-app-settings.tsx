import React from "react";
import PreferencesAppSettingsDesktop from "../preference-app-settings-desktop/preference-app-settings-desktop";
import PreferencesAppSettingsReadonlyFolders from "../preferences-app-settings-readonly-folders/preferences-app-settings-readonly-folders";
import PreferencesAppSettingsStorageFolderMappings from "../preferences-app-settings-storage-folder-mappings/preferences-app-settings-storage-folder-mappings";
import PreferencesAppSettingsStorageFolder from "../preferences-app-settings-storage-folder/preferences-app-settings-storage-folder";

const PreferencesAppSettings: React.FunctionComponent = () => {
  return (
    <div className="preferences--app-settings">
      <div className="content--subheader">AppSettings</div>
      <div className="content--text">
        <PreferencesAppSettingsStorageFolder />
        <PreferencesAppSettingsStorageFolderMappings />
        <PreferencesAppSettingsDesktop />
        <PreferencesAppSettingsReadonlyFolders />
      </div>
    </div>
  );
};

export default PreferencesAppSettings;
