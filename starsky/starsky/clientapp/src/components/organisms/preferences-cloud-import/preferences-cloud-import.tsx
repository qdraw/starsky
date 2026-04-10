import React, { useCallback, useEffect, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { parseRelativeDate } from "../../../shared/date";
import FetchGet from "../../../shared/fetch/fetch-get";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";

interface CloudImportProvider {
  id: string;
  enabled: boolean;
  provider: string;
  remoteFolder: string;
  syncFrequencyMinutes: number;
  syncFrequencyHours: number;
  deleteAfterImport: boolean;
}

interface CloudImportLastSyncResult {
  success?: boolean;
  filesFound?: number;
  filesImportedSuccessfully?: number;
  filesSkipped?: number;
  filesFailed?: number;
  endTime?: string;
}

interface CloudImportStatusResponse {
  providers?: CloudImportProvider[];
  isSyncInProgress?: boolean;
  lastSyncResults?: Record<string, CloudImportLastSyncResult>;
}

const PreferencesCloudImport: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const messageCloudImports = language.key(localization.MessageCloudImports);
  const messageStatusLabel = language.key(localization.MessageCloudImportStatusLabel);
  const messageStatusInProgress = language.key(localization.MessageCloudImportStatusInProgress);
  const messageStatusIdle = language.key(localization.MessageCloudImportStatusIdle);
  const messageStatusUnavailable = language.key(localization.MessageCloudImportStatusUnavailable);
  const messageSyncStartFail = language.key(localization.MessageCloudImportSyncStartFail);
  const messageLoadingStatus = language.key(localization.MessageCloudImportLoadingStatus);
  const messageNoProvidersConfigured = language.key(
    localization.MessageCloudImportNoProvidersConfigured
  );
  const messageProviderId = language.key(localization.MessageCloudImportProviderId);
  const messageEnabled = language.key(localization.MessageCloudImportEnabled);
  const messageDeleteAfterImport = language.key(localization.MessageCloudImportDeleteAfterImport);
  const messageYes = language.key(localization.MessageCloudImportYes);
  const messageNo = language.key(localization.MessageCloudImportNo);
  const messageLastSync = language.key(localization.MessageCloudImportLastSync);
  const messageLastSyncSuccess = language.key(localization.MessageCloudImportLastSyncSuccess);
  const messageLastSyncFailed = language.key(localization.MessageCloudImportLastSyncFailed);
  const messageFilesFound = language.key(localization.MessageCloudImportFilesFound);
  const messageFilesImported = language.key(localization.MessageCloudImportFilesImported);
  const messageFilesSkipped = language.key(localization.MessageCloudImportFilesSkipped);
  const messageFilesFailed = language.key(localization.MessageCloudImportFilesFailed);
  const messageNoSyncResult = language.key(localization.MessageCloudImportNoSyncResult);
  const messageStartSync = language.key(localization.MessageCloudImportStartSync);
  const messageStarting = language.key(localization.MessageCloudImportStarting);
  const MessageDateLessThan1Minute = language.key(localization.MessageDateLessThan1Minute);
  const MessageDateMinutes = language.key(localization.MessageDateMinutes);
  const MessageDateHour = language.key(localization.MessageDateHour);

  const [statusData, setStatusData] = useState<CloudImportStatusResponse | null>(null);
  const [statusError, setStatusError] = useState<string | null>(null);
  const [syncError, setSyncError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isStartingSync, setIsStartingSync] = useState(false);

  const loadStatus = useCallback(async (): Promise<void> => {
    const statusResponse = await FetchGet(new UrlQuery().UrlCloudImportStatus());

    if (statusResponse.statusCode !== 200 || !statusResponse.data) {
      setStatusError(messageStatusUnavailable);
      setIsLoading(false);
      return;
    }

    setStatusData(statusResponse.data as CloudImportStatusResponse);
    setStatusError(null);
    setIsLoading(false);
  }, [messageStatusUnavailable]);

  useEffect(() => {
    const interval = globalThis.setInterval(loadStatus, 10000);
    loadStatus();
    return () => {
      globalThis.clearInterval(interval);
    };
  }, [loadStatus]);

  const isSyncInProgress = statusData?.isSyncInProgress === true;
  const providers = statusData?.providers ?? [];

  const startSync = async (providerId: string): Promise<void> => {
    if (isSyncInProgress || isStartingSync) {
      return;
    }

    setIsStartingSync(true);
    setSyncError(null);

    const response = await FetchPost(new UrlQuery().UrlCloudImportSync(providerId), "");

    if (response.statusCode < 200 || response.statusCode > 299) {
      setSyncError(messageSyncStartFail);
      setIsStartingSync(false);
      return;
    }

    await loadStatus();
    setIsStartingSync(false);
  };

  const statusText = isSyncInProgress ? messageStatusInProgress : messageStatusIdle;

  return (
    <div className="preferences--cloud-import">
      <div className="content--subheader">{messageCloudImports}</div>
      <div className="content--text">
        <div data-test="cloud-import-status" className="preferences-cloud-import-status">
          {messageStatusLabel}: {statusText}
        </div>

        {statusError ? (
          <div data-test="cloud-import-status-error" className="warning-box">
            {statusError}
          </div>
        ) : null}
        {syncError ? (
          <div data-test="cloud-import-sync-error" className="warning-box">
            {syncError}
          </div>
        ) : null}

        {isLoading ? <p>{messageLoadingStatus}</p> : null}

        {!isLoading && providers.length === 0 ? <p>{messageNoProvidersConfigured}</p> : null}

        {providers.map((provider) => {
          const lastSyncResult = statusData?.lastSyncResults?.[provider.id];
          const syncButtonDisabled =
            provider.enabled !== true || isStartingSync || isSyncInProgress;

          return (
            <div
              key={provider.id}
              data-test={`cloud-import-provider-${provider.id}`}
              className="preferences-cloud-import-provider"
            >
              <h4>
                {provider.provider} - {provider.remoteFolder}
              </h4>
              <p>
                {messageProviderId}: {provider.id}
              </p>
              <p>
                {messageEnabled}: {provider.enabled ? messageYes : messageNo}
              </p>
              <p>
                {messageDeleteAfterImport}: {provider.deleteAfterImport ? messageYes : messageNo}
              </p>

              {lastSyncResult ? (
                <div className="preferences-cloud-import-last-sync">
                  <p>
                    {messageLastSync}:{" "}
                    {lastSyncResult.success ? messageLastSyncSuccess : messageLastSyncFailed}
                    {lastSyncResult.endTime
                      ? ` (${language.token(
                          parseRelativeDate(lastSyncResult.endTime, settings.language),
                          ["{lessThan1Minute}", "{minutes}", "{hour}"],
                          [MessageDateLessThan1Minute, MessageDateMinutes, MessageDateHour]
                        )} ${language.key(localization.MessageDateTimeAgoEdited)})`
                      : ""}
                  </p>
                  <p>
                    {messageFilesFound}: {lastSyncResult.filesFound ?? 0}, {messageFilesImported}:{" "}
                    {lastSyncResult.filesImportedSuccessfully ?? 0}, {messageFilesSkipped}:{" "}
                    {lastSyncResult.filesSkipped ?? 0}, {messageFilesFailed}:{" "}
                    {lastSyncResult.filesFailed ?? 0}
                  </p>
                </div>
              ) : (
                <p>{messageNoSyncResult}</p>
              )}

              <button
                className="btn btn--default"
                type="button"
                data-test={`cloud-import-sync-${provider.id}`}
                disabled={syncButtonDisabled}
                onClick={() => startSync(provider.id)}
              >
                {isStartingSync ? messageStarting : messageStartSync}
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default PreferencesCloudImport;
