import React, { useCallback, useEffect, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
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

  const [statusData, setStatusData] = useState<CloudImportStatusResponse | null>(null);
  const [statusError, setStatusError] = useState<string | null>(null);
  const [syncError, setSyncError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isStartingSync, setIsStartingSync] = useState(false);

  const loadStatus = useCallback(async (): Promise<void> => {
    const statusResponse = await FetchGet(new UrlQuery().UrlCloudImportStatus());

    if (statusResponse.statusCode !== 200 || !statusResponse.data) {
      setStatusError("Cloud import status unavailable");
      setIsLoading(false);
      return;
    }

    setStatusData(statusResponse.data as CloudImportStatusResponse);
    setStatusError(null);
    setIsLoading(false);
  }, []);

  useEffect(() => {
    let mounted = true;

    const fetchStatus = async () => {
      if (!mounted) {
        return;
      }
      await loadStatus();
    };

    fetchStatus();
    const interval = window.setInterval(fetchStatus, 10000);

    return () => {
      mounted = false;
      window.clearInterval(interval);
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
      setSyncError("Unable to start cloud import sync");
      setIsStartingSync(false);
      return;
    }

    await loadStatus();
    setIsStartingSync(false);
  };

  const statusText = isSyncInProgress ? "In progress" : "Idle";

  return (
    <div className="preferences--cloud-import">
      <div className="content--subheader">{messageCloudImports}</div>
      <div className="content--text">
        <div data-test="cloud-import-status" className="preferences-cloud-import-status">
          Status: {statusText}
        </div>

        {statusError ? <div className="warning-box">{statusError}</div> : null}
        {syncError ? <div className="warning-box">{syncError}</div> : null}

        {isLoading ? <p>Loading cloud import status...</p> : null}

        {!isLoading && providers.length === 0 ? <p>No providers configured.</p> : null}

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
              <p>ID: {provider.id}</p>
              <p>Enabled: {provider.enabled ? "Yes" : "No"}</p>
              <p>Delete after import: {provider.deleteAfterImport ? "Yes" : "No"}</p>

              {lastSyncResult ? (
                <div className="preferences-cloud-import-last-sync">
                  <p>
                    Last sync: {lastSyncResult.success ? "Success" : "Failed"}
                    {lastSyncResult.endTime ? ` (${lastSyncResult.endTime})` : ""}
                  </p>
                  <p>
                    Files found: {lastSyncResult.filesFound ?? 0}, imported: {" "}
                    {lastSyncResult.filesImportedSuccessfully ?? 0}, skipped: {" "}
                    {lastSyncResult.filesSkipped ?? 0}, failed: {lastSyncResult.filesFailed ?? 0}
                  </p>
                </div>
              ) : (
                <p>No sync result yet.</p>
              )}

              <button
                className="btn btn--default"
                type="button"
                data-test={`cloud-import-sync-${provider.id}`}
                disabled={syncButtonDisabled}
                onClick={() => startSync(provider.id)}
              >
                {isStartingSync ? "Starting..." : "Start sync"}
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default PreferencesCloudImport;