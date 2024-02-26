import { IAppSettingsDefaultEditorApplication } from "./IAppSettingsDefaultEditorApplication";
import { RawJpegMode } from "./ICollectionsOpenType";

export interface IAppSettings {
  verbose: boolean;
  storageFolder: string;
  storageFolderAllowEdit: boolean;
  useLocalDesktop: boolean;
  defaultDesktopEditor: IAppSettingsDefaultEditorApplication[];
  desktopCollectionsOpen: RawJpegMode;
}
