import React, { useEffect, useState } from 'react';
import useFetch from '../../../hooks/use-fetch';
import useGlobalSettings from '../../../hooks/use-global-settings';
import { IAppSettings } from '../../../interfaces/IAppSettings';
import FetchPost from '../../../shared/fetch-post';
import { Language } from '../../../shared/language';
import { UrlQuery } from '../../../shared/url-query';
import FormControl from '../../atoms/form-control/form-control';
import SwitchButton from '../../atoms/switch-button/switch-button';


export const PreferencesAppSettings: React.FunctionComponent<any> = (_) => {

  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageAppSettingsEntireAppScope = language.text("De AppSettings mogen alleen worden aangepast door Administrators. " +
    "Deze instellingen worden toegepast voor de gehele applicatie ",
    "The AppSettings may only be modified by Administrators. These settings are applied for the entire application");
  const MessageChangeNeedReSync = language.text("Je hebt deze instelling veranderd, nu dien je een volledige sync uit te voeren",
    "You have changed this setting, now you need to perform a full sync");

  var permissionsData = useFetch(new UrlQuery().UrlAccountPermissions(), 'get');

  const [enabled, setIsEnabled] = useState(false);

  useEffect(() => {
    function permissions(): boolean {
      if (!permissionsData || !permissionsData.data) return false;
      return permissionsData.data.includes("AppSettingsWrite");
    }
    setIsEnabled(permissions())
  }, [permissionsData])

  async function changeSetting(value: string, name?: string) {
    var bodyParams = new URLSearchParams();
    bodyParams.set(name ? name : '', value);
    await FetchPost(new UrlQuery().UrlApiAppSettings(), bodyParams.toString());
  }

  var appSettings = useFetch(new UrlQuery().UrlApiAppSettings(), 'get').data as IAppSettings | null;

  const [verbose, setIsVerbose] = useState(appSettings?.verbose);
  const [storageFolder, setStorageFolder] = useState(appSettings?.storageFolder);

  useEffect(() => {
    setIsVerbose(appSettings?.verbose);
    setStorageFolder(appSettings?.storageFolder);
  }, [appSettings])

  return <div className="preferences--app-settings">

    <div className="content--subheader">AppSettings</div>
    <div className="content--text">
      <div className="warning-box warning-box--optional">
        {MessageAppSettingsEntireAppScope}
      </div>

      <h4>Verbose logging</h4>

      <SwitchButton isEnabled={enabled} isOn={!verbose} onToggle={(toggle, name) => {
        changeSetting((!toggle).toString(), name);
        setIsVerbose(!toggle);
      }} leftLabel={"on"} name="verbose" rightLabel={"off"} />

      <h4>Storage Folder</h4>
      <FormControl name="storageFolder" onBlur={e => {
        setStorageFolder(e.target.innerText)
        changeSetting(e.target.innerText, 'storageFolder')
      }} contentEditable={enabled}>{storageFolder}</FormControl>
      {storageFolder !== appSettings?.storageFolder ? <div className="warning-box">{MessageChangeNeedReSync}</div> : null}

    </div>
  </div>;
};

export default PreferencesAppSettings
