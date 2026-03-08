import { IAppSettingsDefaultEditorApplication } from "./IAppSettingsDefaultEditorApplication";
import { IAppSettingsPublishProfiles } from "./IAppSettingsPublishProfiles";
import { RawJpegMode } from "./ICollectionsOpenType";

export interface IAppSettings {
  verbose: boolean;
  storageFolder: string;
  storageFolderAllowEdit: boolean;
  useLocalDesktop: boolean;
  defaultDesktopEditor: IAppSettingsDefaultEditorApplication[];
  desktopCollectionsOpen: RawJpegMode;
  publishProfiles?: IAppSettingsPublishProfiles;
}
