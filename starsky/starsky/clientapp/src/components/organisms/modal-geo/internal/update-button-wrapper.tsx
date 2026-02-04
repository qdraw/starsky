import React from "react";
import useGlobalSettings from "../../../../hooks/use-global-settings";
import { IGeoLocationModel } from "../../../../interfaces/IGeoLocationModel";
import { Language } from "../../../../shared/language";
import { ILatLong } from "../modal-geo";
import { UpdateButtonActive } from "./update-button-active";
import { UpdateButtonDisabled } from "./update-button-disabled";

interface IUpdateButtonWrapperProps {
  parentDirectory: string;
  selectedSubPath: string;
  location: ILatLong;
  setError: React.Dispatch<React.SetStateAction<boolean>>;
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  propsCollections?: boolean;
  isLocationUpdated: boolean;
  handleExit: (result: IGeoLocationModel | null) => void;
}

export const UpdateButtonWrapper: React.FC<IUpdateButtonWrapperProps> = ({
  parentDirectory,
  selectedSubPath,
  location,
  setError,
  setIsLoading,
  propsCollections,
  isLocationUpdated,
  handleExit
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  return isLocationUpdated ? (
    <UpdateButtonActive
      parentDirectory={parentDirectory}
      selectedSubPath={selectedSubPath}
      location={location}
      handleExit={handleExit}
      latitude={location.latitude}
      longitude={location.longitude}
      language={language}
      setIsLoading={setIsLoading}
      setError={setError}
      propsCollections={propsCollections}
    />
  ) : (
    <UpdateButtonDisabled
      language={language}
      latitude={location.latitude}
      longitude={location.longitude}
    />
  );
};
