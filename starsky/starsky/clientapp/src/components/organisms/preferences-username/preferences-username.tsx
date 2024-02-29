import React, { useEffect } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";

const PreferencesUsername: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageUnknownUsername = language.key(localization.MessageUnknownUsername);
  const MessageUsername = language.key(localization.MessageUsername);
  const MessageRole = language.key(localization.MessageRole);

  const accountStatus = useFetch(new UrlQuery().UrlAccountStatus(), "get");
  const [userName, setUserName] = React.useState(MessageUnknownUsername);

  useEffect(() => {
    if (
      accountStatus.statusCode !== 200 ||
      !accountStatus.data?.credentialsIdentifiers ||
      accountStatus.data.credentialsIdentifiers.length !== 1
    )
      return;
    setUserName(accountStatus.data.credentialsIdentifiers[0]);
  }, [accountStatus]);

  return (
    <>
      <div className="content--subheader">{MessageUsername}</div>
      <div
        data-test="preferences-username-text"
        className="content--text preferences-username-text"
      >
        {userName}
      </div>
      <div className="content--subheader">{MessageRole}</div>
      <div className="content--text preferences-role">{accountStatus?.data?.roleCode}</div>
    </>
  );
};

export default PreferencesUsername;
