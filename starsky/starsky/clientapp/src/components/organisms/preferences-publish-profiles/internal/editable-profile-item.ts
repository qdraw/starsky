import { IAppSettingsPublishProfileItem } from "../../../../interfaces/IAppSettingsPublishProfiles";

export interface EditableProfileItem extends IAppSettingsPublishProfileItem {
  _id: string;
  optimizersText: string;
}