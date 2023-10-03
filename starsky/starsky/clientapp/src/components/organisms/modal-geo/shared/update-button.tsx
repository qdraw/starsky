import { IGeoLocationModel } from "../../../../interfaces/IGeoLocationModel";
import localization from "../../../../localization/localization.json";
import { Language } from "../../../../shared/language";
import { ILatLong } from "../modal-geo";
import { UpdateGeoLocation } from "./update-geo-location";

export class UpdateButton {
  parentDirectory: string;
  selectedSubPath: string;
  location: ILatLong;
  setError: any;
  setIsLoading: any;
  propsCollections: boolean | undefined;

  constructor(
    parentDirectory: string,
    selectedSubPath: string,
    location: ILatLong,
    setError: React.Dispatch<React.SetStateAction<boolean>>,
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>,
    propsCollections?: boolean | undefined
  ) {
    this.parentDirectory = parentDirectory;
    this.selectedSubPath = selectedSubPath;
    this.location = location;
    this.setError = setError;
    this.setIsLoading = setIsLoading;
    this.propsCollections = propsCollections;
  }
  updateButton(
    isLocationUpdated: boolean,
    handleExit: (result: IGeoLocationModel | null) => void,
    latitude: number | undefined,
    longitude: number | undefined,
    language: Language
  ): React.JSX.Element {
    return isLocationUpdated ? (
      <button
        onClick={async () => {
          const model = await UpdateGeoLocation(
            this.parentDirectory,
            this.selectedSubPath,
            this.location,
            this.setError,
            this.setIsLoading,
            this.propsCollections
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
    ) : (
      <button className="btn btn--default" disabled={true}>
        {/* disabled */}
        {!latitude && !longitude
          ? language.key(localization.MessageAddLocation)
          : language.key(localization.MessageUpdateLocation)}
      </button>
    );
  }
}
