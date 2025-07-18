import React from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";

const PreferencesUsername: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageUsername = language.key(localization.MessageUsername);
  const MessageRole = language.key(localization.MessageRole);

  interface AccountStatusData {
    credentialsIdentifiers?: string[];
    roleCode: string;
  }

  const accountStatus = useFetch(new UrlQuery().UrlAccountStatus(), "get");
  const accountStatusData = accountStatus?.data as AccountStatusData;

  let userName = language.key(localization.MessageUnknownUsername);

  if (
    accountStatus.statusCode === 200 &&
    accountStatusData?.credentialsIdentifiers &&
    accountStatusData?.credentialsIdentifiers.length >= 1
  ) {
    userName = accountStatusData?.credentialsIdentifiers[0];
    if (userName === "mail@localhost") {
      userName = language.key(localization.MessageDesktopMailLocalhostUsername);
    }
  }

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
      <div className="content--text preferences-role">{accountStatusData?.roleCode}</div>
    </>
  );
};

export default PreferencesUsername;
