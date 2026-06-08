import React, { useState } from "react";
import DropArea from "../../atoms/drop-area/drop-area";
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

  const getErrorContext = (statusCode: number, data: unknown): string => {
    const parts: string[] = [];

    if (typeof data === "string" && data.trim().length > 0) {
      parts.push(data.trim());
    }

    if (data && typeof data === "object") {
      const candidateData = data as Record<string, unknown>;
      const candidates = ["message", "error", "title", "detail"];
      candidates.forEach((key) => {
        const value = candidateData[key];
        if (typeof value === "string" && value.trim().length > 0) {
          parts.push(value.trim());
        }
      });
    }

    const uniqueParts = [...new Set(parts)];
    const text = uniqueParts.join(" | ");

    if (text.length > 0) {
      return `HTTP ${statusCode}: ${text}`;
    }

    return `HTTP ${statusCode}`;
  };

  const mapStatusToError = (statusCode: number, fallback: string, data: unknown): string => {
    if (statusCode === 401 || statusCode === 403) {
      return messageAdminOnly;
    }

    return `${fallback} (${getErrorContext(statusCode, data)})`;
  };

  const readFileAsText = async (file: File): Promise<string> => {
    if (typeof file.text === "function") {
      return file.text();
    }

    return new Promise((resolve, reject) => {
      const fileReader = new FileReader();
      fileReader.onload = () => {
        resolve(typeof fileReader.result === "string" ? fileReader.result : "");
      };
      fileReader.onerror = () => {
        reject(new Error("Unable to read file"));
      };
      fileReader.readAsText(file);
    });
  };

  const importJson = async (): Promise<void> => {
    if (!selectedFile) {
      setError(messageNoFileSelected);
      return;
    }

    setIsImporting(true);

    let rawJson = "";
    try {
      rawJson = await readFileAsText(selectedFile);
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
      setError(mapStatusToError(response.statusCode, messageImportFail, response.data));
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
      let errorData: unknown = null;
      try {
        errorData = await response.text();
      } catch {
        errorData = null;
      }

      setError(mapStatusToError(response.status, messageExportFail, errorData));
      setIsExporting(false);
      return;
    }

    const jsonPayload = await response.text();

    try {
      const jsonBlob = new Blob([jsonPayload], { type: "application/json" });
      const objectUrl =
        typeof URL.createObjectURL === "function" ? URL.createObjectURL(jsonBlob) : "";

      const downloadLink = document.createElement("a");
      downloadLink.href = objectUrl;
      downloadLink.download = "import-index-export.json";
      document.body.appendChild(downloadLink);
      downloadLink.click();
      document.body.removeChild(downloadLink);

      if (objectUrl && typeof URL.revokeObjectURL === "function") {
        URL.revokeObjectURL(objectUrl);
      }
    } catch {
      setError(messageExportFail);
      setIsExporting(false);
      return;
    }

    setSuccess(messageExportSuccess);
    setIsExporting(false);
  };

  return (
    <div className="preferences--import-index-json">
      <div className="content--header">{messageImportIndexJson}</div>
      <div className="content--text">
        <p>{messageDescription}</p>
        <p>{messageAdminOnly}</p>

        <DropArea
          enableInputButton={true}
          enableDragAndDrop={false}
          className="btn btn--default"
          inputButtonLabel={messageSelectFile}
          accept="application/json,.json"
          inputDisabled={isImporting || isExporting}
          onFilesSelected={(files) => {
            setSelectedFile(files?.[0] ?? null);
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
      <div className="content--header">{messageExport}</div>

      <div className="content--text">

      <button
        className="btn btn--default"
        type="button"
        data-test="import-index-json-export-button"
        disabled={isImporting || isExporting}
        onClick={exportJson}
      >
        {isExporting ? messageExporting : messageExport}
      </button>
    </div>
    </div>
  );
};

export default PreferencesImportIndexJson;
