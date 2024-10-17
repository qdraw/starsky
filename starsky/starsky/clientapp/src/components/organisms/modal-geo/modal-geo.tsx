import L from "leaflet";
import React, { useCallback, useEffect, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IGeoLocationModel } from "../../../interfaces/IGeoLocationModel";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import FormControl from "../../atoms/form-control/form-control";
import Modal from "../../atoms/modal/modal";
import Preloader from "../../atoms/preloader/preloader";
import { LatLongRound } from "./internal/lat-long-round";
import { RealtimeMapUpdate } from "./internal/realtime-map-update";
import { UpdateButtonWrapper } from "./internal/update-button-wrapper";
import { UpdateMap } from "./internal/update-map";

interface IModalMoveFileProps {
  isOpen: boolean;
  isFormEnabled: boolean;
  handleExit: (result: IGeoLocationModel | null) => void;
  selectedSubPath: string;
  parentDirectory: string;
  latitude?: number;
  longitude?: number;
  collections?: boolean;
}

export interface ILatLong {
  latitude: number;
  longitude: number;
}

const ModalGeo: React.FunctionComponent<IModalMoveFileProps> = ({
  latitude,
  longitude,
  isFormEnabled,
  parentDirectory,
  selectedSubPath,
  ...props
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const [error, setError] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const [mapState, setMapState] = useState<L.Map | null>(null);

  const [location, setLocation] = useState<ILatLong>({
    latitude: LatLongRound(latitude),
    longitude: LatLongRound(longitude)
  });

  useEffect(() => {
    if (mapState === null || !latitude || !longitude) {
      return;
    }

    if (location.latitude === latitude && location.longitude === longitude) {
      return;
    }

    RealtimeMapUpdate(
      mapState,
      isFormEnabled,
      setLocation,
      setIsLocationUpdated,
      latitude,
      longitude
    );

    // when get new location
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [latitude, longitude]);

  const [isLocationUpdated, setIsLocationUpdated] = useState<boolean>(false);

  const mapReference = useCallback((node: HTMLDivElement | null) => {
    if (node !== null && mapState === null) {
      UpdateMap(node, location, isFormEnabled, setLocation, setIsLocationUpdated, setMapState);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function subHeader(): string {
    let message: string;
    if (isFormEnabled) {
      if (!latitude && !longitude) {
        message = language.key(localization.MessageAddLocation);
      } else {
        message = language.key(localization.MessageUpdateLocation);
      }
    } else {
      message = language.key(localization.MessageViewLocation);
    }
    return message;
  }

  return (
    <Modal
      id="move-geo"
      className="modal-bg-large"
      isOpen={props.isOpen}
      handleExit={() => props.handleExit(null)}
    >
      <div className="content" data-test="modal-geo">
        {isLoading ? <Preloader isWhite={false} isOverlay={true} /> : null}

        <div className="modal content--subheader">{subHeader()}</div>
        {error ? (
          <div className="modal modal-button-bar-error">
            <div data-test="login-error" className="content--error-true">
              {language.key(localization.MessageErrorGenericFail)}
            </div>
          </div>
        ) : null}
        <div className="content-geo" data-test="content-geo" ref={mapReference}></div>
        <div className="modal modal-button-bar">
          <button
            data-test="force-cancel"
            onClick={() => props.handleExit(null)}
            className="btn btn--info"
          >
            {language.key(localization.MessageCancel)}
          </button>
          {isFormEnabled ? (
            <UpdateButtonWrapper
              handleExit={props.handleExit}
              isLocationUpdated={isLocationUpdated}
              parentDirectory={parentDirectory}
              location={location}
              selectedSubPath={selectedSubPath}
              setError={setError}
              setIsLoading={setIsLoading}
              propsCollections={props.collections}
            />
          ) : null}
          <div className="lat-long">
            <b>Latitude:</b>{" "}
            <FormControl
              contentEditable={false}
              data-test="modal-latitude"
              className={"inline"}
              name={"lat"}
            >
              {location.latitude}
            </FormControl>{" "}
            <b>Longitude:</b>{" "}
            <FormControl
              contentEditable={false}
              className={"inline"}
              data-test="modal-longitude"
              name={"lat"}
            >
              {location.longitude}
            </FormControl>
          </div>
        </div>
      </div>
    </Modal>
  );
};

export default ModalGeo;
