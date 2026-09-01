import React, { useEffect, useRef, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IAppSettings } from "../../../interfaces/IAppSettings";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import FormControl from "../../atoms/form-control/form-control";

type FolderRow = { id: number; folder: string };

export async function ChangeSettingReadOnlyFolders(folders: FolderRow[]): Promise<number> {
  const bodyParams = new URLSearchParams();
  folders
    .filter((r) => r.folder)
    .forEach((r, idx) => {
      bodyParams.append(`ReadOnlyFolders[${idx}]`, r.folder);
    });
  const result = await FetchPost(new UrlQuery().UrlApiAppSettings(), bodyParams.toString());
  return result?.statusCode;
}

const PreferencesAppSettingsReadonlyFolders: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageAppSettingsReadOnlyFolders = language.key(
    localization.MessageAppSettingsReadOnlyFolders
  );
  const MessageAppSettingsReadOnlyFoldersNone = language.key(
    localization.MessageAppSettingsReadOnlyFoldersNone
  );
  const MessageAppSettingsReadOnlyFoldersAdd = language.key(
    localization.MessageAppSettingsReadOnlyFoldersAdd
  );
  const MessageAppSettingsReadOnlyFoldersRemove = language.key(
    localization.MessageAppSettingsReadOnlyFoldersRemove
  );
  const MessageAppSettingsReadOnlyFoldersError = language.key(
    localization.MessageAppSettingsReadOnlyFoldersError
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

  const [rows, setRows] = useState<FolderRow[]>([]);
  const nextId = useRef(0);
  const [changed, setChanged] = useState(false);
  const [saveError, setSaveError] = useState(false);

  useEffect(() => {
    if (!appSettings?.readOnlyFolders) return;
    setRows(appSettings.readOnlyFolders.map((folder) => ({ id: nextId.current++, folder })));
  }, [appSettings]);

  async function saveAll(updated: FolderRow[]) {
    try {
      const statusCode = await ChangeSettingReadOnlyFolders(updated);
      if (statusCode === 200) {
        setSaveError(false);
        setChanged(true);
        return;
      }
      setSaveError(true);
      setChanged(false);
    } catch {
      setSaveError(true);
      setChanged(false);
    }
  }

  function handleBlur(id: number, value: string) {
    const updated = rows.map((r) => (r.id === id ? { ...r, folder: value } : r));
    setRows(updated);
    saveAll(updated);
  }

  function addRow() {
    setRows((prev) => [...prev, { id: nextId.current++, folder: "" }]);
  }

  function removeRow(id: number) {
    const updated = rows.filter((r) => r.id !== id);
    setRows(updated);
    saveAll(updated);
  }

  return (
    <>
      <div className="content--subheader">{MessageAppSettingsReadOnlyFolders}</div>

      {rows.length === 0 && !isEnabled ? (
        <p data-test="readonly-folders-none">{MessageAppSettingsReadOnlyFoldersNone}</p>
      ) : null}
      {rows.map((row) => (
        <div
          key={row.id}
          className="preferences--readonly-folder-row"
          data-test="readonly-folder-row"
        >
          <FormControl
            name={`readOnlyFolders-${row.id}`}
            className="form-control inline-block form-control--half"
            contentEditable={isEnabled}
            onBlur={(e) => handleBlur(row.id, e.target.innerText)}
            spellcheck={false}
          >
            {row.folder}
          </FormControl>
          {isEnabled ? (
            <button
              type="button"
              className="btn btn--default inline-block"
              data-test={`readonly-folder-remove-${row.id}`}
              onClick={() => removeRow(row.id)}
            >
              {MessageAppSettingsReadOnlyFoldersRemove}
            </button>
          ) : null}
        </div>
      ))}
      {isEnabled ? (
        <button
          type="button"
          className="btn btn--default"
          data-test="readonly-folder-add"
          onClick={addRow}
        >
          {MessageAppSettingsReadOnlyFoldersAdd}
        </button>
      ) : null}
      {saveError ? (
        <div className="warning-box warning-box--error" data-test="readonly-folders-error">
          {MessageAppSettingsReadOnlyFoldersError}
        </div>
      ) : null}
      {changed ? (
        <div className="warning-box" data-test="readonly-folders-changed">
          {MessageChangeNeedReSync}{" "}
          <a target="_blank" href={new UrlQuery().DocsGettingStartedFirstSteps()} rel="noreferrer">
            {MessageReadMoreHere}
          </a>
        </div>
      ) : null}
    </>
  );
};

export default PreferencesAppSettingsReadonlyFolders;
