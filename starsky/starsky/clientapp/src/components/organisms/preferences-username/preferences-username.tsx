import React, { useEffect } from 'react';
import useFetch from '../../../hooks/use-fetch';
import useGlobalSettings from '../../../hooks/use-global-settings';
import { Language } from '../../../shared/language';
import { UrlQuery } from '../../../shared/url-query';

export const PreferencesUsername: React.FunctionComponent<any> = (_) => {

  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageUnknownUsername = language.text("Onbekende gebruikersnaam", "Unknown username");
  const MessageUsername = language.text("Gebruikersnaam", "Username");

  var accountStatus = useFetch(new UrlQuery().UrlAccountStatus(), 'get');
  const [userName, setUserName] = React.useState(MessageUnknownUsername);

  useEffect(() => {
    if (accountStatus.statusCode !== 200 || !accountStatus.data
      || !accountStatus.data.credentials || accountStatus.data.credentials.length !== 1) return;
    setUserName(accountStatus.data.credentials[0].identifier);
  }, [accountStatus])

  return (<>
    <div className="content--subheader">{MessageUsername}</div>
    <div className="content--text">
      {userName}
    </div>
  </>);
};
