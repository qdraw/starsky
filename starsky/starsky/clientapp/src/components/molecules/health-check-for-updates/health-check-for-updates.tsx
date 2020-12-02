import React from "react";
import useFetch from "../../../hooks/use-fetch";
import useGlobalSettings from "../../../hooks/use-global-settings";
import BrowserDetect from "../../../shared/browser-detect";
import { DifferenceInDate } from "../../../shared/date";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url-query";
import Notification, {
  NotificationType
} from "../../atoms/notification/notification";

/**
 * Name of Storage Name
 */
export const CheckForUpdatesLocalStorageName = "HealthCheckForUpdates";

/**
 * Skip display of message
 */
export function SkipDisplayOfUpdate(): boolean {
  const localStorageItem = localStorage.getItem(
    CheckForUpdatesLocalStorageName
  );
  if (!localStorageItem) return false;

  var getItem = parseInt(localStorageItem);
  if (isNaN(getItem)) return false;
  return DifferenceInDate(getItem) < 5760; // 4 days
}

/**
 * Compontent with health check for updates
 */
const HealthCheckForUpdates: React.FunctionComponent = () => {
  const checkForUpdates = useFetch(
    new UrlQuery().UrlHealthCheckForUpdates(),
    "get"
  );

  const settings = useGlobalSettings();

  if (SkipDisplayOfUpdate() || checkForUpdates.statusCode !== 202) return null;

  var language = new Language(settings.language);

  const ReleasesUrlToken =
    "<a target='_blank' href='https://github.com/qdraw/starsky/releases/latest' " +
    "rel='noopener'> {releasesToken}</a>";
  let WhereToFindRelease = language.token(
    ReleasesUrlToken,
    ["{releasesToken}"],
    [
      language.text(
        "Ga naar het release overzicht",
        "Go to the release overview"
      )
    ]
  );
  if (new BrowserDetect().IsElectronApp())
    WhereToFindRelease = language.text(
      "Ga naar het Help menu en dan release overzicht",
      "Go to the release overview"
    );

  let MessageNewVersionUpdateToken = language.text(
    "Er is een nieuwe versie beschikbaar {WhereToFindRelease}",
    "A new version is available {WhereToFindRelease}"
  );

  const MessageNewVersionUpdateHtml = language.token(
    MessageNewVersionUpdateToken,
    ["{WhereToFindRelease}"],
    [WhereToFindRelease]
  );

  return (
    <Notification
      callback={() => {
        localStorage.setItem(
          CheckForUpdatesLocalStorageName,
          Date.now().toString()
        );
      }}
      type={NotificationType.default}
    >
      <div
        dangerouslySetInnerHTML={{ __html: MessageNewVersionUpdateHtml }}
      ></div>
    </Notification>
  );
};

export default HealthCheckForUpdates;
