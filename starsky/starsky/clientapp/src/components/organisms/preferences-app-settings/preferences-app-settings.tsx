import React, { useEffect, useState } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IAppSettings } from "../../../interfaces/IAppSettings";
import FetchPost from "../../../shared/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url-query";
import FormControl from "../../atoms/form-control/form-control";
import SwitchButton from "../../atoms/switch-button/switch-button";

export async function ChangeSetting(
  value: string,
  name?: string
): Promise<number> {
  const bodyParams = new URLSearchParams();
  bodyParams.set(name ?? "", value);
  const result = await FetchPost(
    new UrlQuery().UrlApiAppSettings(),
    bodyParams.toString()
  );
  return result?.statusCode;
}

export const PreferencesAppSettings: React.FunctionComponent<any> = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageAppSettingsEntireAppScope = language.text(
    "De AppSettings mogen alleen worden aangepast door Administrators. " +
      "Deze instellingen worden toegepast voor de gehele applicatie ",
    "The AppSettings may only be modified by Administrators. These settings are applied for the entire application"
  );
  const MessageChangeNeedReSync = language.text(
    "Je hebt deze instelling veranderd, nu dien je een volledige sync uit te voeren. Ga naar de hoofdmap, het meer-menu en klik op handmatig synchroniseren.",
    "You have changed this setting, now you need to perform a full sync. Go to the root folder, the more menu and click on manual sync."
  );

  const permissionsData = useFetch(
    new UrlQuery().UrlAccountPermissions(),
    "get"
  );

  const [enabled, setIsEnabled] = useState(false);

  useEffect(() => {
    function permissions(): boolean {
      if (
        !permissionsData?.data?.includes ||
        permissionsData?.statusCode !== 200
      ) {
        return false;
      }
      return permissionsData.data.includes("AppSettingsWrite");
    }
    setIsEnabled(permissions());
  }, [permissionsData]);

  const [storageFolderNotFound, setStorageFolderNotFound] = useState(false);

  const appSettings = useFetch(new UrlQuery().UrlApiAppSettings(), "get")
    ?.data as IAppSettings | null;

  const [verbose, setIsVerbose] = useState(appSettings?.verbose);
  const [storageFolder, setStorageFolder] = useState(
    appSettings?.storageFolder
  );

  useEffect(() => {
    setIsVerbose(appSettings?.verbose);
    setStorageFolder(appSettings?.storageFolder);
  }, [appSettings]);

  return (
    <div className="preferences--app-settings">
      <div className="content--subheader">AppSettings</div>
      <div className="content--text">
        <div className="warning-box warning-box--optional">
          {MessageAppSettingsEntireAppScope}
        </div>

        <h4>Verbose logging</h4>

        <SwitchButton
          isEnabled={enabled}
          isOn={!verbose}
          onToggle={(toggle, name) => {
            ChangeSetting((!toggle).toString(), name);
            setIsVerbose(!toggle);
          }}
          leftLabel={"on"}
          name="verbose"
          rightLabel={"off"}
        />

        <h4>Storage Folder</h4>
        <FormControl
          name="storageFolder"
          onBlur={async (e) => {
            const resultStatusCode = await ChangeSetting(
              e.target.innerText,
              "storageFolder"
            );
            setStorageFolder(e.target.innerText);
            setStorageFolderNotFound(resultStatusCode === 404);
          }}
          contentEditable={
            enabled && appSettings?.storageFolderAllowEdit === true
          }
        >
          {storageFolder}
        </FormControl>

        {storageFolderNotFound ? (
          <div className="warning-box" data-test="storage-not-found">
            Directory not found so not saved
          </div>
        ) : null}

        {storageFolder !== appSettings?.storageFolder &&
        !storageFolderNotFound ? (
          <div className="warning-box" data-test="storage-changed">
            {MessageChangeNeedReSync}
          </div>
        ) : null}

        {appSettings?.storageFolderAllowEdit !== true ? (
          <div className="warning-box" data-test="storage-env">
            You should update the Environment variable app__storageFolder
          </div>
        ) : null}
      </div>
    </div>
  );
};

export default PreferencesAppSettings;
