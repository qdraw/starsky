import React from 'react';
import MenuDefault from '../components/organisms/menu-default/menu-default';
import PreferencesAppSettings from '../components/organisms/preferences-app-settings/preferences-app-settings';
import { PreferencesPassword } from '../components/organisms/preferences-password/preferences-password';
import useGlobalSettings from '../hooks/use-global-settings';
import { Language } from '../shared/language';

export const Preferences: React.FunctionComponent<any> = (_) => {

  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessagePreferences = language.text("Voorkeuren", "Preferences");

  return <>
    <MenuDefault isEnabled={true} />
    <div className="content--header">{MessagePreferences}</div>

    <PreferencesPassword />
    <PreferencesAppSettings />

  </>;
};

export default Preferences
