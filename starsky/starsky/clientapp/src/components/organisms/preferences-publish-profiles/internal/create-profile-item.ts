import { ITemplateContentType, IAppSettingsPublishProfileItem } from "../../../../interfaces/IAppSettingsPublishProfiles";
import { EditableProfileItem } from "./editable-profile-item";

export const createProfileItem = (
  templateContentTypes: ITemplateContentType[],
  item?: IAppSettingsPublishProfileItem
): EditableProfileItem => {
  const defaultContentType = templateContentTypes[0]?.type ?? "Html";
  const toBoolean = (value: unknown, fallback: boolean): boolean => {
    if (typeof value === "boolean") {
      return value;
    }
    if (typeof value === "string") {
      return value.toLowerCase() === "true";
    }
    return fallback;
  };

  return {
    _id: `${Date.now()}-${Math.random().toString(36).slice(2)}`,
    contentType: item?.contentType ?? defaultContentType,
    sourceMaxWidth: item?.sourceMaxWidth ?? 0,
    overlayMaxWidth: item?.overlayMaxWidth ?? 0,
    overlayFullPath: item?.overlayFullPath ?? item?.path ?? "",
    path: item?.path ?? "",
    prepend: item?.prepend ?? "",
    template: item?.template ?? "",
    copy: toBoolean(item?.copy, true),
    optimizers: item?.optimizers ?? [],
    optimizersText: JSON.stringify(item?.optimizers ?? [], null, 2),
    folder: item?.folder ?? "",
    append: item?.append ?? "",
    metaData: toBoolean(item?.metaData, true)
  };
};
