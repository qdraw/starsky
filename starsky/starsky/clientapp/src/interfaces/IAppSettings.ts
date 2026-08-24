import { IAppSettingsDefaultEditorApplication } from "./IAppSettingsDefaultEditorApplication";
import { RawJpegMode } from "./ICollectionsOpenType";

export interface IAppSettings {
  verbose: boolean;
  storageFolder: string;
  storageFolderAllowEdit: boolean;
  storageFolderMappings: Record<string, string>;
  useLocalDesktop: boolean;
  defaultDesktopEditor: IAppSettingsDefaultEditorApplication[];
  desktopCollectionsOpen: RawJpegMode;
}
