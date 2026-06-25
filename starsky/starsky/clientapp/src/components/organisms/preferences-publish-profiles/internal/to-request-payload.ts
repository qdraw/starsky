import { IAppSettingsPublishProfiles, IAppSettingsPublishProfileItem } from "../../../../interfaces/IAppSettingsPublishProfiles";
import { EditableProfile } from "./editable-profile";

export  const toRequestPayload = (
    setHasValidationError: React.Dispatch<React.SetStateAction<boolean>>, 
    setFeedbackMessage: React.Dispatch<React.SetStateAction<string>>, 
    messageInvalidOptimizers: string,
    profiles: EditableProfile[]): { 
    publishProfiles: IAppSettingsPublishProfiles } | null => {
    const publishProfiles: IAppSettingsPublishProfiles = {};

    for (const profile of profiles) {
      const profileName = profile.name.trim();
      if (!profileName) {
        continue;
      }

      const items: IAppSettingsPublishProfileItem[] = [];

      for (const item of profile.items) {
        let parsedOptimizers: NonNullable<IAppSettingsPublishProfileItem["optimizers"]> =
          item.optimizers ?? [];

        if (item.optimizersText.trim()) {
          try {
            const parsed = JSON.parse(item.optimizersText) as unknown;
            parsedOptimizers = Array.isArray(parsed)
              ? ((parsed as IAppSettingsPublishProfileItem["optimizers"]) ?? [])
              : [];
          } catch {
            setHasValidationError(true);
            setFeedbackMessage(messageInvalidOptimizers);
            return null;
          }
        }

        const mapped: IAppSettingsPublishProfileItem = {
          contentType: item.contentType,
          sourceMaxWidth: Number(item.sourceMaxWidth ?? 0),
          overlayMaxWidth: Number(item.overlayMaxWidth ?? 0),
          path: item.overlayFullPath?.trim() ? item.overlayFullPath : item.path,
          prepend: item.prepend,
          template: item.template,
          copy: item.copy === true,
          optimizers: parsedOptimizers,
          folder: item.folder,
          append: item.append,
          metaData: item.metaData === true
        };

        items.push(mapped);
      }

      publishProfiles[profileName] = items;
    }

    return { publishProfiles };
  };