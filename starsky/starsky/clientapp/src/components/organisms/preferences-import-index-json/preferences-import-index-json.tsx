import React, { useState } from "react";
import FetchPost from "../../../shared/fetch/fetch-post";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";

const PreferencesImportIndexJson: React.FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const messageImportIndexJson = language.key(localization.MessageImportIndexJson);
  const messageDescription = language.key(localization.MessageImportIndexJsonDescription);
  const messageAdminOnly = language.key(localization.MessageImportIndexJsonAdminOnly);
  const messageSelectFile = language.key(localization.MessageImportIndexJsonSelectFile);
  const messageNoFileSelected = language.key(localization.MessageImportIndexJsonNoFileSelected);
  const messageImport = language.key(localization.MessageImportIndexJsonImport);
  const messageImporting = language.key(localization.MessageImportIndexJsonImporting);
  const messageExport = language.key(localization.MessageImportIndexJsonExport);
  const messageExporting = language.key(localization.MessageImportIndexJsonExporting);
  const messageImportSuccess = language.key(localization.MessageImportIndexJsonImportSuccess);
  const messageImportFail = language.key(localization.MessageImportIndexJsonImportFail);
  const messageExportSuccess = language.key(localization.MessageImportIndexJsonExportSuccess);
  const messageExportFail = language.key(localization.MessageImportIndexJsonExportFail);
  const messageInvalidJson = language.key(localization.MessageImportIndexJsonInvalidJson);

  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isImporting, setIsImporting] = useState(false);
  const [isExporting, setIsExporting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const setError = (message: string) => {
    setSuccessMessage(null);
    setErrorMessage(message);
  };

  const setSuccess = (message: string) => {
    setErrorMessage(null);
    setSuccessMessage(message);
  };

  const mapStatusToError = (statusCode: number, fallback: string): string => {
    if (statusCode === 401 || statusCode === 403) {
      return messageAdminOnly;
    }

    return fallback;
  };

  const importJson = async (): Promise<void> => {
    if (!selectedFile) {
      setError(messageNoFileSelected);
      return;
    }

    setIsImporting(true);

    let rawJson = "";
    try {
      rawJson = await selectedFile.text();
      JSON.parse(rawJson);
    } catch {
      setError(messageInvalidJson);
      setIsImporting(false);
      return;
    }

    const response = await FetchPost(
      new UrlQuery().UrlImportIndexJsonImport(),
      rawJson,
      "post",
      {
        "Content-Type": "application/json"
      }
    );

    if (response.statusCode >= 200 && response.statusCode <= 299) {
      setSuccess(messageImportSuccess);
    } else {
      setError(mapStatusToError(response.statusCode, messageImportFail));
    }

    setIsImporting(false);
  };

  const exportJson = async (): Promise<void> => {
    setIsExporting(true);

    let response: Response;
    try {
      response = await fetch(new UrlQuery().UrlImportIndexJsonExport(), {
        method: "GET",
        credentials: "include",
        headers: {
          Accept: "application/json",
          "X-Requested-With": "XMLHttpRequest"
        }
      });
    } catch {
      setError(messageExportFail);
      setIsExporting(false);
      return;
    }

    if (!response.ok) {
      setError(mapStatusToError(response.status, messageExportFail));
      setIsExporting(false);
      return;
    }

    const jsonPayload = await response.text();

    const downloadLink = document.createElement("a");
    downloadLink.href = `data:application/json;charset=utf-8,${encodeURIComponent(jsonPayload)}`;
    downloadLink.download = "import-index-export.json";
    document.body.appendChild(downloadLink);
    downloadLink.click();
    document.body.removeChild(downloadLink);

    setSuccess(messageExportSuccess);
    setIsExporting(false);
  };

  return (
    <div className="preferences--import-index-json">
      <div className="content--subheader">{messageImportIndexJson}</div>
      <div className="content--text">
        <p>{messageDescription}</p>
        <p>{messageAdminOnly}</p>

        <label htmlFor="import-index-json-file">{messageSelectFile}</label>
        <input
          id="import-index-json-file"
          data-test="import-index-json-file"
          type="file"
          accept="application/json,.json"
          disabled={isImporting || isExporting}
          onChange={(event) => {
            setSelectedFile(event.target.files?.[0] ?? null);
            setErrorMessage(null);
            setSuccessMessage(null);
          }}
        />

        <p data-test="import-index-json-file-name">
          {selectedFile ? selectedFile.name : messageNoFileSelected}
        </p>

        <button
          className="btn btn--default"
          type="button"
          data-test="import-index-json-import-button"
          disabled={!selectedFile || isImporting || isExporting}
          onClick={importJson}
        >
          {isImporting ? messageImporting : messageImport}
        </button>

        <button
          className="btn btn--default"
          type="button"
          data-test="import-index-json-export-button"
          disabled={isImporting || isExporting}
          onClick={exportJson}
        >
          {isExporting ? messageExporting : messageExport}
        </button>

        {errorMessage ? (
          <div data-test="import-index-json-error" className="warning-box">
            {errorMessage}
          </div>
        ) : null}

        {successMessage ? (
          <div data-test="import-index-json-success" className="warning-box warning-box--optional">
            {successMessage}
          </div>
        ) : null}
      </div>
    </div>
  );
};

export default PreferencesImportIndexJson;
