import { FunctionComponent } from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { BrowserDetect } from "../../../shared/browser-detect";
import { DifferenceInDate } from "../../../shared/date";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import Notification, { NotificationType } from "../../atoms/notification/notification";

/**
 * Name of Storage Name
 */
export const CheckForUpdatesLocalStorageName = "HealthCheckForUpdates";

/**
 * Skip display of message
 */
export function SkipDisplayOfUpdate(): boolean {
  const localStorageItem = localStorage.getItem(CheckForUpdatesLocalStorageName);
  if (!localStorageItem) return false;

  const getItem = parseInt(localStorageItem);
  if (isNaN(getItem)) return false;
  return DifferenceInDate(getItem) < 5760; // 4 days
}

/**
 * Component with health check for updates
 */
const HealthCheckForUpdates: FunctionComponent = () => {
  const checkForUpdates = useFetch(new UrlQuery().UrlHealthCheckForUpdates(), "get");
  const releaseInfo = useFetch(
    new UrlQuery().UrlHealthReleaseInfo(checkForUpdates.data as string),
    "get"
  );

  const settings = useGlobalSettings();

  if (SkipDisplayOfUpdate() || checkForUpdates.statusCode !== 202) return null;

  const language = new Language(settings.language);

  let WhereToFindRelease = language.key(
    localization.MessageWhereToFindReleaseReleasesUrlTokenHtml,
    ["{releasesToken}"],
    [language.key(localization.MessageWhereToFindReleaseReleasesUrlTokenContent)]
  );

  if (new BrowserDetect().IsElectronApp()) {
    WhereToFindRelease = language.key(localization.WhereToFindReleaseElectronApp);
  }

  const MessageNewVersionUpdateToken = language.key(localization.MessageNewVersionUpdateToken);

  const MessageNewVersionUpdateHtml = language.token(
    MessageNewVersionUpdateToken,
    ["{WhereToFindRelease}", "{otherInfo}"],
    [WhereToFindRelease, releaseInfo.data as string]
  );

  return (
    <Notification
      callback={() => {
        localStorage.setItem(CheckForUpdatesLocalStorageName, Date.now().toString());
      }}
      type={NotificationType.default}
    >
      <div dangerouslySetInnerHTML={{ __html: MessageNewVersionUpdateHtml }}></div>
    </Notification>
  );
};

export default HealthCheckForUpdates;
