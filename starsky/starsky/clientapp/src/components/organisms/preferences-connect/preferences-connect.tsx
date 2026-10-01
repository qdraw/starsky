import React, { useCallback, useEffect, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import FetchGet from "../../../shared/fetch/fetch-get";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";

interface ConnectDevice {
  id: string;
  name: string;
  addresses: string[];
}

interface ConnectFolder {
  id: string;
  label: string;
  path: string;
  type: string;
}

interface ConnectConfigResponse {
  deviceId: string;
  deviceName: string;
  devices: ConnectDevice[];
  folders: ConnectFolder[];
}

const PreferencesConnect: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const messageConnect = language.key(localization.MessageConnect);
  const messageDeviceId = language.key(localization.MessageConnectDeviceId);
  const messageDeviceName = language.key(localization.MessageConnectDeviceName);
  const messageDevices = language.key(localization.MessageConnectDevices);
  const messageNoDevices = language.key(localization.MessageConnectNoDevices);
  const messageAddDevice = language.key(localization.MessageConnectAddDevice);
  const messagePlaceholderId = language.key(localization.MessageConnectAddDevicePlaceholderId);
  const messagePlaceholderName = language.key(localization.MessageConnectAddDevicePlaceholderName);
  const messageRemove = language.key(localization.MessageConnectRemove);
  const messageCopy = language.key(localization.MessageConnectCopy);
  const messageCopied = language.key(localization.MessageConnectCopied);
  const messageLoading = language.key(localization.MessageConnectLoading);
  const messageError = language.key(localization.MessageConnectError);
  const messageAddError = language.key(localization.MessageConnectAddError);
  const messageRemoveError = language.key(localization.MessageConnectRemoveError);

  const [config, setConfig] = useState<ConnectConfigResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  const [newDeviceId, setNewDeviceId] = useState("");
  const [newDeviceName, setNewDeviceName] = useState("");
  const [isAdding, setIsAdding] = useState(false);
  const [addError, setAddError] = useState<string | null>(null);
  const [removeError, setRemoveError] = useState<string | null>(null);

  const loadConfig = useCallback(async (): Promise<void> => {
    setIsLoading(true);
    const result = await FetchGet(new UrlQuery().UrlConnectConfig());
    if (result.statusCode !== 200 || !result.data) {
      setLoadError(messageError);
      setIsLoading(false);
      return;
    }
    setConfig(result.data as ConnectConfigResponse);
    setLoadError(null);
    setIsLoading(false);
  }, [messageError]);

  useEffect(() => {
    loadConfig();
  }, [loadConfig]);

  const copyDeviceId = async (): Promise<void> => {
    if (!config?.deviceId) return;
    await navigator.clipboard.writeText(config.deviceId);
    setCopied(true);
    globalThis.setTimeout(() => setCopied(false), 2000);
  };

  const addDevice = async (): Promise<void> => {
    if (!newDeviceId.trim()) return;
    setIsAdding(true);
    setAddError(null);

    const body = JSON.stringify({ deviceId: newDeviceId.trim(), name: newDeviceName.trim() });
    const result = await FetchPost(new UrlQuery().UrlConnectDevice(), body, "post", {
      "Content-Type": "application/json"
    });

    if (result.statusCode < 200 || result.statusCode > 299) {
      setAddError(messageAddError);
      setIsAdding(false);
      return;
    }

    setNewDeviceId("");
    setNewDeviceName("");
    await loadConfig();
    setIsAdding(false);
  };

  const removeDevice = async (deviceId: string): Promise<void> => {
    setRemoveError(null);
    const result = await FetchPost(
      new UrlQuery().UrlConnectDeviceDelete(deviceId),
      "",
      "delete"
    );

    if (result.statusCode < 200 || result.statusCode > 299) {
      setRemoveError(messageRemoveError);
      return;
    }

    await loadConfig();
  };

  return (
    <div className="preferences--connect">
      <div className="content--subheader">{messageConnect}</div>
      <div className="content--text">
        {isLoading && <p data-test="connect-loading">{messageLoading}</p>}

        {loadError && (
          <div data-test="connect-load-error" className="warning-box">
            {loadError}
          </div>
        )}

        {config && (
          <>
            <div className="preferences-connect-section">
              <p className="preferences-connect-label">{messageDeviceName}</p>
              <p data-test="connect-device-name" className="preferences-connect-value">
                {config.deviceName}
              </p>
            </div>

            <div className="preferences-connect-section">
              <p className="preferences-connect-label">{messageDeviceId}</p>
              <div className="preferences-connect-device-id">
                <code data-test="connect-device-id" className="preferences-connect-code">
                  {config.deviceId}
                </code>
                <button
                  type="button"
                  className="btn btn--default preferences-connect-copy"
                  data-test="connect-copy-device-id"
                  onClick={copyDeviceId}
                >
                  {copied ? messageCopied : messageCopy}
                </button>
              </div>
            </div>

            <div className="preferences-connect-section">
              <p className="preferences-connect-label">{messageDevices}</p>

              {removeError && (
                <div data-test="connect-remove-error" className="warning-box">
                  {removeError}
                </div>
              )}

              {config.devices.length === 0 ? (
                <p data-test="connect-no-devices">{messageNoDevices}</p>
              ) : (
                <ul className="preferences-connect-devices">
                  {config.devices.map((device) => (
                    <li
                      key={device.id}
                      data-test={`connect-device-${device.id}`}
                      className="preferences-connect-device"
                    >
                      <span className="preferences-connect-device-name">
                        {device.name || device.id}
                      </span>
                      <code className="preferences-connect-code preferences-connect-code--small">
                        {device.id}
                      </code>
                      <button
                        type="button"
                        className="btn btn--danger"
                        data-test={`connect-remove-device-${device.id}`}
                        onClick={() => removeDevice(device.id)}
                      >
                        {messageRemove}
                      </button>
                    </li>
                  ))}
                </ul>
              )}

              <div className="preferences-connect-add-device">
                <input
                  type="text"
                  className="form-control"
                  data-test="connect-new-device-id"
                  placeholder={messagePlaceholderId}
                  value={newDeviceId}
                  onChange={(e) => setNewDeviceId(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") addDevice();
                  }}
                />
                <input
                  type="text"
                  className="form-control"
                  data-test="connect-new-device-name"
                  placeholder={messagePlaceholderName}
                  value={newDeviceName}
                  onChange={(e) => setNewDeviceName(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") addDevice();
                  }}
                />
                {addError && (
                  <div data-test="connect-add-error" className="warning-box">
                    {addError}
                  </div>
                )}
                <button
                  type="button"
                  className="btn btn--default"
                  data-test="connect-add-device"
                  disabled={isAdding || !newDeviceId.trim()}
                  onClick={addDevice}
                >
                  {messageAddDevice}
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default PreferencesConnect;
