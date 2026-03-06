import React, { useEffect, useMemo, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IAppSettings } from "../../../interfaces/IAppSettings";
import {
  IAppSettingsPublishProfileItem,
  IAppSettingsPublishProfiles,
  ITemplateContentType
} from "../../../interfaces/IAppSettingsPublishProfiles";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";

interface EditableProfileItem extends IAppSettingsPublishProfileItem {
  _id: string;
  optimizersText: string;
}

interface EditableProfile {
  name: string;
  items: EditableProfileItem[];
}

const createProfileItem = (
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

const createProfiles = (
  publishProfiles: IAppSettingsPublishProfiles | undefined,
  templateContentTypes: ITemplateContentType[]
): EditableProfile[] => {
  if (!publishProfiles || Object.keys(publishProfiles).length === 0) {
    return [
      {
        name: "_default",
        items: [createProfileItem(templateContentTypes)]
      }
    ];
  }

  return Object.entries(publishProfiles).map(([name, items]) => ({
    name,
    items:
      items.length > 0
        ? items.map((item) => createProfileItem(templateContentTypes, item))
        : [createProfileItem(templateContentTypes)]
  }));
};

const PreferencesPublishProfiles: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const messagePublishProfiles = language.key(localization.MessagePublishProfiles);
  const messagePublishProfilesDescription = language.key(
    localization.MessagePublishProfilesDescription
  );
  const messagePublishProfile = language.key(localization.MessagePublishProfile);
  const messagePublishProfileName = language.key(localization.MessagePublishProfilesProfileName);
  const messagePublishProfileItems = language.key(localization.MessagePublishProfileItems);
  const messagePublishContentType = language.key(localization.MessagePublishContentType);
  const messageSourceMaxWidth = language.key(localization.MessagePublishSourceMaxWidth);
  const messageOverlayMaxWidth = language.key(localization.MessagePublishOverlayMaxWidth);
  const messageOverlayFullPath = language.key(localization.MessagePublishOverlayFullPath);
  const messagePath = language.key(localization.MessagePath);
  const messagePrepend = language.key(localization.MessagePublishPrepend);
  const messageTemplate = language.key(localization.MessagePublishTemplate);
  const messageCopy = language.key(localization.MessagePublishCopy);
  const messageOptimizers = language.key(localization.MessagePublishOptimizers);
  const messageFolder = language.key(localization.MessageFolder);
  const messageAppend = language.key(localization.MessagePublishAppend);
  const messageMetaData = language.key(localization.MessagePublishMetaData);
  const messageSave = language.key(localization.MessageSave);
  const messageAddProfile = language.key(localization.MessagePublishAddProfile);
  const messageAddContentType = language.key(localization.MessagePublishAddContentType);
  const messageRemoveProfile = language.key(localization.MessagePublishRemoveProfile);
  const messageRemoveContentType = language.key(localization.MessagePublishRemoveContentType);
  const messageSaveSuccess = language.key(localization.MessagePublishProfilesSaveSuccess);
  const messageSaveError = language.key(localization.MessagePublishProfilesSaveError);
  const messageInvalidOptimizers = language.key(localization.MessagePublishInvalidOptimizers);
  const messageLoading = language.key(localization.MessagePublishProfilesLoading);
  const messageNoTypes = language.key(localization.MessagePublishProfilesNoContentTypes);

  const appSettingsResult = useFetch(new UrlQuery().UrlApiAppSettings(), "get");
  const permissionsData = useFetch(new UrlQuery().UrlAccountPermissions(), "get");
  const templateTypesResult = useFetch(new UrlQuery().UrlApiTemplateContentTypes(), "get");

  const appSettings = appSettingsResult.data as IAppSettings | null;
  const templateContentTypes = (templateTypesResult.data as ITemplateContentType[] | null) ?? [];

  const isAppSettingsWrite =
    Array.isArray(permissionsData?.data) &&
    permissionsData.data.includes(new UrlQuery().KeyAccountPermissionAppSettingsWrite());

  const [profiles, setProfiles] = useState<EditableProfile[]>([]);
  const [activeProfileIndex, setActiveProfileIndex] = useState(0);
  const [feedbackMessage, setFeedbackMessage] = useState("");
  const [hasValidationError, setHasValidationError] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (templateContentTypes.length === 0) {
      return;
    }

    setProfiles(createProfiles(appSettings?.publishProfiles, templateContentTypes));
  }, [appSettings, templateContentTypes]);

  const activeProfile = profiles[activeProfileIndex];

  const contentTypesByName = useMemo(() => {
    const values = new Map<string, ITemplateContentType>();
    templateContentTypes.forEach((contentType) => values.set(contentType.type, contentType));
    return values;
  }, [templateContentTypes]);

  const updateProfileName = (index: number, value: string) => {
    setProfiles((current) =>
      current.map((profile, profileIndex) =>
        profileIndex === index ? { ...profile, name: value } : profile
      )
    );
  };

  const updateItemField = <K extends keyof EditableProfileItem>(
    itemId: string,
    field: K,
    value: EditableProfileItem[K]
  ) => {
    setProfiles((current) =>
      current.map((profile, profileIndex) => {
        if (profileIndex !== activeProfileIndex) {
          return profile;
        }

        return {
          ...profile,
          items: profile.items.map((item) => {
            if (item._id !== itemId) {
              return item;
            }

            return {
              ...item,
              [field]: value
            };
          })
        };
      })
    );
  };

  const addProfile = () => {
    if (templateContentTypes.length === 0) {
      return;
    }

    setProfiles((current) => {
      const nextProfiles = [
        ...current,
        {
          name: `profile_${current.length + 1}`,
          items: [createProfileItem(templateContentTypes)]
        }
      ];
      setActiveProfileIndex(nextProfiles.length - 1);
      return nextProfiles;
    });
  };

  const removeProfile = () => {
    if (profiles.length <= 1) {
      return;
    }

    setProfiles((current) => current.filter((_, index) => index !== activeProfileIndex));
    setActiveProfileIndex((current) => Math.max(0, current - 1));
  };

  const addContentType = () => {
    setProfiles((current) =>
      current.map((profile, index) => {
        if (index !== activeProfileIndex) {
          return profile;
        }

        return {
          ...profile,
          items: [...profile.items, createProfileItem(templateContentTypes)]
        };
      })
    );
  };

  const removeContentType = (itemId: string) => {
    setProfiles((current) =>
      current.map((profile, index) => {
        if (index !== activeProfileIndex || profile.items.length <= 1) {
          return profile;
        }

        return {
          ...profile,
          items: profile.items.filter((item) => item._id !== itemId)
        };
      })
    );
  };

  const toRequestPayload = (): { publishProfiles: IAppSettingsPublishProfiles } | null => {
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

  const savePublishProfiles = async () => {
    setHasValidationError(false);
    setFeedbackMessage("");

    const payload = toRequestPayload();
    if (!payload) {
      return;
    }

    setIsSaving(true);

    const result = await FetchPost(new UrlQuery().UrlApiAppSettings(), JSON.stringify(payload), "post", {
      "Content-Type": "application/json"
    });

    setIsSaving(false);

    if (result.statusCode >= 200 && result.statusCode < 300) {
      setFeedbackMessage(messageSaveSuccess);
      return;
    }

    setHasValidationError(true);
    setFeedbackMessage(messageSaveError);
  };

  if (appSettingsResult.statusCode === 999 || templateTypesResult.statusCode === 999) {
    return (
      <div className="preferences--publish-profiles">
        <div className="content--subheader">{messagePublishProfiles}</div>
        <p>{messageLoading}</p>
      </div>
    );
  }

  if (templateContentTypes.length === 0) {
    return (
      <div className="preferences--publish-profiles">
        <div className="content--subheader">{messagePublishProfiles}</div>
        <p>{messageNoTypes}</p>
      </div>
    );
  }

  return (
    <div className="preferences--publish-profiles">
      <div className="content--subheader">{messagePublishProfiles}</div>
      <div className="content--text">
        <div className="warning-box warning-box--optional">{messagePublishProfilesDescription}</div>

        <div className="preferences-publish-profiles-toolbar">
          <button
            type="button"
            className="btn btn--default"
            onClick={addProfile}
            disabled={!isAppSettingsWrite}
            data-test="publish-profiles-add-profile"
          >
            {messageAddProfile}
          </button>

          <button
            type="button"
            className="btn btn--default"
            onClick={removeProfile}
            disabled={!isAppSettingsWrite || profiles.length <= 1}
            data-test="publish-profiles-remove-profile"
          >
            {messageRemoveProfile}
          </button>
        </div>

        <div className="preferences-publish-profiles-selector">
          <label htmlFor="publish-profile-select">{messagePublishProfile}</label>
          <select
            id="publish-profile-select"
            className="select"
            value={activeProfileIndex}
            onChange={(event) => setActiveProfileIndex(Number(event.target.value))}
            data-test="publish-profiles-select-profile"
          >
            {profiles.map((profile, index) => (
              <option value={index} key={`${profile.name}-${index}`}>
                {profile.name || `profile_${index + 1}`}
              </option>
            ))}
          </select>
        </div>

        {activeProfile ? (
          <div className="preferences-publish-profiles-profile">
            <label htmlFor="publish-profile-name">{messagePublishProfileName}</label>
            <input
              id="publish-profile-name"
              className="form-control"
              type="text"
              value={activeProfile.name}
              onChange={(event) => updateProfileName(activeProfileIndex, event.target.value)}
              disabled={!isAppSettingsWrite}
              data-test="publish-profiles-name"
            />

            <div className="preferences-publish-profiles-toolbar">
              <button
                type="button"
                className="btn btn--default"
                onClick={addContentType}
                disabled={!isAppSettingsWrite}
                data-test="publish-profiles-add-content-type"
              >
                {messageAddContentType}
              </button>
            </div>

            <div className="preferences-publish-profiles-items-header">{messagePublishProfileItems}</div>

            {activeProfile.items.map((item) => {
              const currentType = contentTypesByName.get(item.contentType) ?? templateContentTypes[0];
              const properties = currentType?.properties;

              return (
                <div className="preferences-publish-profiles-item" key={item._id}>
                  <div className="preferences-publish-profiles-grid">
                    <label>{messagePublishContentType}</label>
                    <select
                      className="select"
                      value={item.contentType}
                      onChange={(event) => updateItemField(item._id, "contentType", event.target.value)}
                      disabled={!isAppSettingsWrite}
                    >
                      {templateContentTypes.map((contentType) => (
                        <option key={contentType.id} value={contentType.type}>
                          {contentType.type}
                        </option>
                      ))}
                    </select>

                    {properties?.sourceMaxWidth ? (
                      <>
                        <label>{messageSourceMaxWidth}</label>
                        <input
                          className="form-control"
                          type="number"
                          value={item.sourceMaxWidth ?? 0}
                          onChange={(event) =>
                            updateItemField(item._id, "sourceMaxWidth", Number(event.target.value))
                          }
                          disabled={!isAppSettingsWrite}
                        />
                      </>
                    ) : null}

                    {properties?.overlayMaxWidth ? (
                      <>
                        <label>{messageOverlayMaxWidth}</label>
                        <input
                          className="form-control"
                          type="number"
                          value={item.overlayMaxWidth ?? 0}
                          onChange={(event) =>
                            updateItemField(item._id, "overlayMaxWidth", Number(event.target.value))
                          }
                          disabled={!isAppSettingsWrite}
                        />
                      </>
                    ) : null}

                    {properties?.path ? (
                      <>
                        <label>{messagePath}</label>
                        <input
                          className="form-control"
                          type="text"
                          value={item.path ?? ""}
                          onChange={(event) => updateItemField(item._id, "path", event.target.value)}
                          disabled={!isAppSettingsWrite}
                        />
                      </>
                    ) : null}

                    {properties?.overlayFullPath ? (
                      <>
                        <label>{messageOverlayFullPath}</label>
                        <input
                          className="form-control"
                          type="text"
                          value={item.overlayFullPath ?? ""}
                          onChange={(event) =>
                            updateItemField(item._id, "overlayFullPath", event.target.value)
                          }
                          disabled={!isAppSettingsWrite}
                        />
                      </>
                    ) : null}

                    {properties?.prepend ? (
                      <>
                        <label>{messagePrepend}</label>
                        <input
                          className="form-control"
                          type="text"
                          value={item.prepend ?? ""}
                          onChange={(event) => updateItemField(item._id, "prepend", event.target.value)}
                          disabled={!isAppSettingsWrite}
                        />
                      </>
                    ) : null}

                    {properties?.template ? (
                      <>
                        <label>{messageTemplate}</label>
                        <input
                          className="form-control"
                          type="text"
                          value={item.template ?? ""}
                          onChange={(event) => updateItemField(item._id, "template", event.target.value)}
                          disabled={!isAppSettingsWrite}
                        />
                      </>
                    ) : null}

                    {properties?.folder ? (
                      <>
                        <label>{messageFolder}</label>
                        <input
                          className="form-control"
                          type="text"
                          value={item.folder ?? ""}
                          onChange={(event) => updateItemField(item._id, "folder", event.target.value)}
                          disabled={!isAppSettingsWrite}
                        />
                      </>
                    ) : null}

                    {properties?.append ? (
                      <>
                        <label>{messageAppend}</label>
                        <input
                          className="form-control"
                          type="text"
                          value={item.append ?? ""}
                          onChange={(event) => updateItemField(item._id, "append", event.target.value)}
                          disabled={!isAppSettingsWrite}
                        />
                      </>
                    ) : null}

                    {properties?.copy ? (
                      <>
                        <label>{messageCopy}</label>
                        <input
                          type="checkbox"
                          checked={item.copy === true}
                          onChange={(event) => updateItemField(item._id, "copy", event.target.checked)}
                          disabled={!isAppSettingsWrite}
                        />
                      </>
                    ) : null}

                    {properties?.metaData ? (
                      <>
                        <label>{messageMetaData}</label>
                        <input
                          type="checkbox"
                          checked={item.metaData === true}
                          onChange={(event) =>
                            updateItemField(item._id, "metaData", event.target.checked)
                          }
                          disabled={!isAppSettingsWrite}
                        />
                      </>
                    ) : null}

                    {properties?.optimizers ? (
                      <>
                        <label>{messageOptimizers}</label>
                        <textarea
                          className="form-control"
                          value={item.optimizersText}
                          onChange={(event) =>
                            updateItemField(item._id, "optimizersText", event.target.value)
                          }
                          disabled={!isAppSettingsWrite}
                          rows={6}
                        />
                      </>
                    ) : null}
                  </div>

                  <button
                    type="button"
                    className="btn btn--default"
                    onClick={() => removeContentType(item._id)}
                    disabled={!isAppSettingsWrite || activeProfile.items.length <= 1}
                    data-test="publish-profiles-remove-content-type"
                  >
                    {messageRemoveContentType}
                  </button>
                </div>
              );
            })}
          </div>
        ) : null}

        {feedbackMessage ? (
          <div
            className={hasValidationError ? "warning-box" : "warning-box warning-box--optional"}
            data-test="publish-profiles-feedback"
          >
            {feedbackMessage}
          </div>
        ) : null}

        <button
          type="button"
          className="btn btn--default"
          onClick={savePublishProfiles}
          disabled={!isAppSettingsWrite || isSaving}
          data-test="publish-profiles-save"
        >
          {isSaving ? `${messageSave}...` : messageSave}
        </button>
      </div>
    </div>
  );
};

export default PreferencesPublishProfiles;
