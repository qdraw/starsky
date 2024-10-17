import { IGeoLocationModel } from "../../../../interfaces/IGeoLocationModel";
import localization from "../../../../localization/localization.json";
import { Language } from "../../../../shared/language";
import { ILatLong } from "../modal-geo";
import { UpdateGeoLocation } from "./update-geo-location";

interface IUpdateButtonActiveProps {
  parentDirectory: string;
  handleExit: (result: IGeoLocationModel | null) => void;
  selectedSubPath: string;
  latitude?: number;
  longitude?: number;
  location: ILatLong;
  language: Language;
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
  setError: React.Dispatch<React.SetStateAction<boolean>>;
  propsCollections: boolean | undefined;
}

export const UpdateButtonActive: React.FunctionComponent<IUpdateButtonActiveProps> = ({
  handleExit,
  parentDirectory,
  latitude,
  longitude,
  language,
  ...props
}) => {
  return (
    <button
      onClick={async () => {
        const model = await UpdateGeoLocation(
          parentDirectory,
          props.selectedSubPath,
          props.location,
          props.setError,
          props.setIsLoading,
          props.propsCollections
        );
        if (model) {
          handleExit(model);
        }
      }}
      data-test="update-geo-location"
      className="btn btn--default"
    >
      {!latitude && !longitude
        ? language.key(localization.MessageAddLocation)
        : language.key(localization.MessageUpdateLocation)}
    </button>
  );
};
