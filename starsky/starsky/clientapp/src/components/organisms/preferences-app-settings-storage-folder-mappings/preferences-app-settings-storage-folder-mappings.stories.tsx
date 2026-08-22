import PreferencesAppSettingsStorageFolderMappings
  from "./preferences-app-settings-storage-folder-mappings.tsx";

export default {
  title: "preferences-app-settings-storage-folder-mappings"
};

export const Default = () => {
  return (
    <div data-test="preferences-username-text" className="content--text preferences-username-text">
      <PreferencesAppSettingsStorageFolderMappings/>
    </div>
  );
};

Default.storyName = "default";
