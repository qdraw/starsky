import React, { useState, useEffect } from "react";
import { useFolderPicker } from "../../../hooks/use-folder-picker";
import FormControl from "../form-control/form-control";

interface IFolderPickerInputProps {
  value: string;
  onChange: (value: string) => void;
  onBlur?: (e: React.ChangeEvent<HTMLDivElement>) => void;
  isEnabled: boolean;
  allowEdit: boolean;
  "data-test"?: string;
}

/**
 * FolderPickerInput Component
 * 
 * Provides a folder selection input that:
 * - Uses native folder picker on macOS WKWebView and Windows WebView2
 * - Falls back to contentEditable FormControl for browsers
 */
const FolderPickerInput: React.FunctionComponent<IFolderPickerInputProps> = ({
  value,
  onChange,
  onBlur,
  isEnabled,
  allowEdit,
  "data-test": dataTest
}) => {
  const { isNativeApp, requestFolderSelection } = useFolderPicker();
  const [displayValue, setDisplayValue] = useState(value);

  useEffect(() => {
    setDisplayValue(value);
  }, [value]);

  const handleOpenFolderPicker = () => {
    requestFolderSelection((folderPath) => {
      if (folderPath) {
        setDisplayValue(folderPath);
        onChange(folderPath);
      }
    });
  };

  // Native app with folder picker capability
  if (isEnabled && allowEdit && isNativeApp()) {
    return (
      <div data-test={dataTest} className="folder-picker-input">
        <div className="folder-picker-input__display">{displayValue}</div>
        <button
          type="button"
          onClick={handleOpenFolderPicker}
          className="folder-picker-input__button"
        >
          Browse...
        </button>
      </div>
    );
  }

  // Fallback: contentEditable FormControl
  return (
    <FormControl
      name="storageFolder"
      onBlur={async (e) => {
        const newValue = (e.target as HTMLDivElement).innerText;
        setDisplayValue(newValue);
        onChange(newValue);
        onBlur?.(e);
      }}
      contentEditable={isEnabled && allowEdit}
      data-test={dataTest}
    >
      {displayValue}
    </FormControl>
  );
};

export default FolderPickerInput;
