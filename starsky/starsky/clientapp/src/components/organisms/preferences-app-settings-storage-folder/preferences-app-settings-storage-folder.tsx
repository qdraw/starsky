import React, { useEffect, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IAppSettings } from "../../../interfaces/IAppSettings";
import localization from "../../../localization/localization.json";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import FormControl from "../../atoms/form-control/form-control";

/**
 * Update Change Settings
 * @param value - content
 * @param name - key name
 * @returns void
 */
export async function ChangeSetting(value: string, name?: string): Promise<number> {
  const bodyParams = new URLSearchParams();
  bodyParams.set(name ?? "", value);
  const result = await FetchPost(new UrlQuery().UrlApiAppSettings(), bodyParams.toString());
  return result?.statusCode;
}

const PreferencesAppSettingsStorageFolder: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageAppSettingsEntireAppScope = language.key(
    localization.MessageAppSettingsEntireAppScope
  );
  const MessageChangeNeedReSync = language.key(localization.MessageChangeNeedReSync);
  const MessageAppSettingsStorageFolder = language.key(
    localization.MessageAppSettingsStorageFolder
  );
  const MessageAppSettingStorageFolderSaveFail = language.key(
    localization.MessageAppSettingStorageFolderSaveFail
  );
  const MessageAppSettingStorageFolderEnvUsedFail = language.key(
    localization.MessageAppSettingStorageFolderEnvUsedFail
  );
  const MessageReadMoreHere = language.key(localization.MessageReadMoreHere);
  const permissionsData = useFetch(new UrlQuery().UrlAccountPermissions(), "get");

  const [isEnabled, setIsEnabled] = useState(false);

  useEffect(() => {
    function permissions(): boolean {
      const data = permissionsData?.data as string[];
      if (!data?.includes || permissionsData?.statusCode !== 200) {
        return false;
      }
      // AppSettingsWrite
      return data.includes(new UrlQuery().KeyAccountPermissionAppSettingsWrite());
    }

    setIsEnabled(permissions());
  }, [permissionsData]);

  const [storageFolderNotFound, setStorageFolderNotFound] = useState(false);

  const appSettings = useFetch(new UrlQuery().UrlApiAppSettings(), "get")
    ?.data as IAppSettings | null;

  const [storageFolder, setStorageFolder] = useState(appSettings?.storageFolder);

  useEffect(() => {
    setStorageFolder(appSettings?.storageFolder);
  }, [appSettings]);

  // macOS native folder picker: register the callback that Swift evaluates after the
  // NSOpenPanel resolves. Only active when running inside WKWebView (window.webkit exists).
  const isMacWebView =
    typeof window !== "undefined" && "webkit" in window;

  useEffect(() => {
    if (!isMacWebView) return;
    (window as unknown as Record<string, unknown>)["__starskyStorageFolderSelected"] =
      async (path: string) => {
        const resultStatusCode = await ChangeSetting(path, "storageFolder");
        setStorageFolder(path);
        setStorageFolderNotFound(resultStatusCode === 404);
      };
    return () => {
      delete (window as unknown as Record<string, unknown>)["__starskyStorageFolderSelected"];
    };
  }, [isMacWebView]);

  return (
    <>
      <div className={isEnabled ? "warning-box warning-box--optional" : "warning-box"}>
        {MessageAppSettingsEntireAppScope}
      </div>
      <h4>{MessageAppSettingsStorageFolder} </h4>
      <FormControl
        name="storageFolder"
        onBlur={async (e) => {
          const resultStatusCode = await ChangeSetting(e.target.innerText, "storageFolder");
          setStorageFolder(e.target.innerText);
          setStorageFolderNotFound(resultStatusCode === 404);
        }}
        contentEditable={isEnabled && appSettings?.storageFolderAllowEdit === true}
      >
        {storageFolder}
      </FormControl>

      {isMacWebView && isEnabled && appSettings?.storageFolderAllowEdit === true ? (
        <button
          data-test="storage-folder-browse"
          onClick={() => {
            (
              window as unknown as {
                webkit: { messageHandlers: { storageFolderPicker: { postMessage: (v: unknown) => void } } };
              }
            ).webkit.messageHandlers.storageFolderPicker.postMessage({});
          }}
        >
          Browse…
        </button>
      ) : null}

      {storageFolderNotFound ? (
        <div className="warning-box" data-test="storage-not-found">
          {MessageAppSettingStorageFolderSaveFail}
        </div>
      ) : null}

      {storageFolder !== appSettings?.storageFolder && !storageFolderNotFound ? (
        <div className="warning-box" data-test="storage-changed">
          {MessageChangeNeedReSync}{" "}
          <a target="_blank" href={new UrlQuery().DocsGettingStartedFirstSteps()} rel="noreferrer">
            {MessageReadMoreHere}
          </a>
        </div>
      ) : null}

      {appSettings?.storageFolderAllowEdit === true ? null : (
        <div className="warning-box" data-test="storage-env">
          {MessageAppSettingStorageFolderEnvUsedFail}
        </div>
      )}
    </>
  );
};

export default PreferencesAppSettingsStorageFolder;
