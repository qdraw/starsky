import PreferencesAppSettingsStorageFolder from "./preferences-app-settings-storage-folder";

export default {
  title: "components/organisms/preferences-app-settings-storage-folder"
};

export const Default = () => {
  return (
    <div data-test="preferences-username-text" className="content--text preferences-username-text">
      <PreferencesAppSettingsStorageFolder />
    </div>
  );
};

Default.storyName = "default";
