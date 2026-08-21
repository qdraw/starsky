import React, { useEffect, useRef, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IAppSettings } from "../../../interfaces/IAppSettings";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import FormControl from "../../atoms/form-control/form-control";

type MappingRow = { id: number; subPath: string; physicalPath: string };

export async function ChangeSettingMappings(mappings: MappingRow[]): Promise<number> {
  const bodyParams = new URLSearchParams();
  let idx = 0;
  for (const { subPath, physicalPath } of mappings) {
    if (subPath && physicalPath) {
      bodyParams.append(`StorageFolderMappings[${idx}].Key`, subPath);
      bodyParams.append(`StorageFolderMappings[${idx}].Value`, physicalPath);
      idx++;
    }
  }
  const result = await FetchPost(new UrlQuery().UrlApiAppSettings(), bodyParams.toString());
  return result?.statusCode;
}

const PreferencesAppSettingsStorageFolderMappings: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageAppSettingsStorageFolderMappings = language.key(
    localization.MessageAppSettingsStorageFolderMappings
  );
  const MessageAppSettingsStorageFolderMappingsSubPath = language.key(
    localization.MessageAppSettingsStorageFolderMappingsSubPath
  );
  const MessageAppSettingsStorageFolderMappingsPhysicalPath = language.key(
    localization.MessageAppSettingsStorageFolderMappingsPhysicalPath
  );
  const MessageAppSettingsStorageFolderMappingsAdd = language.key(
    localization.MessageAppSettingsStorageFolderMappingsAdd
  );
  const MessageAppSettingsStorageFolderMappingsRemove = language.key(
    localization.MessageAppSettingsStorageFolderMappingsRemove
  );
  const MessageChangeNeedReSync = language.key(localization.MessageChangeNeedReSync);
  const MessageReadMoreHere = language.key(localization.MessageReadMoreHere);

  const permissionsData = useFetch(new UrlQuery().UrlAccountPermissions(), "get");
  const [isEnabled, setIsEnabled] = useState(false);

  useEffect(() => {
    const data = permissionsData?.data as string[];
    if (!data?.includes || permissionsData?.statusCode !== 200) {
      setIsEnabled(false);
      return;
    }
    setIsEnabled(data.includes(new UrlQuery().KeyAccountPermissionAppSettingsWrite()));
  }, [permissionsData]);

  const appSettings = useFetch(new UrlQuery().UrlApiAppSettings(), "get")
    ?.data as IAppSettings | null;

  const [mappings, setMappings] = useState<MappingRow[]>([]);
  const nextId = useRef(0);
  const [changed, setChanged] = useState(false);

  useEffect(() => {
    if (!appSettings?.storageFolderMappings) return;
    const entries = Object.entries(appSettings.storageFolderMappings);
    setMappings(
      entries.map(([subPath, physicalPath]) => ({
        id: nextId.current++,
        subPath,
        physicalPath
      }))
    );
  }, [appSettings]);

  async function saveAll(rows: MappingRow[]) {
    await ChangeSettingMappings(rows);
    setChanged(true);
  }

  function handleSubPathBlur(id: number, value: string) {
    const updated = mappings.map((r) => (r.id === id ? { ...r, subPath: value } : r));
    setMappings(updated);
    saveAll(updated);
  }

  function handlePhysicalPathBlur(id: number, value: string) {
    const updated = mappings.map((r) => (r.id === id ? { ...r, physicalPath: value } : r));
    setMappings(updated);
    saveAll(updated);
  }

  function addRow() {
    setMappings((prev) => [...prev, { id: nextId.current++, subPath: "", physicalPath: "" }]);
  }

  function removeRow(id: number) {
    const updated = mappings.filter((r) => r.id !== id);
    setMappings(updated);
    saveAll(updated);
  }

  return (
    <>
      <h4>{MessageAppSettingsStorageFolderMappings}</h4>
      {mappings.map((row) => (
        <div
          key={row.id}
          className="preferences--storage-folder-mapping-row"
          data-test="storage-folder-mapping-row"
        >
          <FormControl
            name={`storageFolderMappings-subPath-${row.id}`}
            className="form-control inline-block form-control--half"
            contentEditable={isEnabled}
            onBlur={(e) => handleSubPathBlur(row.id, e.target.innerText)}
            placeholder={MessageAppSettingsStorageFolderMappingsSubPath}
            spellcheck={false}
          >
            {row.subPath}
          </FormControl>
          <span className="preferences--storage-folder-mapping-arrow">→</span>
          <FormControl
            name={`storageFolderMappings-physicalPath-${row.id}`}
            className="form-control inline-block form-control--half"
            contentEditable={isEnabled}
            onBlur={(e) => handlePhysicalPathBlur(row.id, e.target.innerText)}
            placeholder={MessageAppSettingsStorageFolderMappingsPhysicalPath}
            spellcheck={false}
          >
            {row.physicalPath}
          </FormControl>
          {isEnabled ? (
            <button
              type="button"
              className="btn btn--default"
              data-test={`storage-folder-mapping-remove-${row.id}`}
              onClick={() => removeRow(row.id)}
            >
              {MessageAppSettingsStorageFolderMappingsRemove}
            </button>
          ) : null}
        </div>
      ))}
      {isEnabled ? (
        <button
          type="button"
          className="btn btn--default"
          data-test="storage-folder-mapping-add"
          onClick={addRow}
        >
          {MessageAppSettingsStorageFolderMappingsAdd}
        </button>
      ) : null}
      {changed ? (
        <div className="warning-box" data-test="storage-mapping-changed">
          {MessageChangeNeedReSync}{" "}
          <a target="_blank" href={new UrlQuery().DocsGettingStartedFirstSteps()} rel="noreferrer">
            {MessageReadMoreHere}
          </a>
        </div>
      ) : null}
    </>
  );
};

export default PreferencesAppSettingsStorageFolderMappings;
