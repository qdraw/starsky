import L from "leaflet";
import React, { useCallback, useEffect, useState } from "react";
import { IGeoLocationModel } from "../../../interfaces/IGeoLocationModel";

import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";
import FormControl from "../../atoms/form-control/form-control";
import Modal from "../../atoms/modal/modal";
import Preloader from "../../atoms/preloader/preloader";
import { LatLongRound } from "./shared/lat-long-round";
import { RealtimeMapUpdate } from "./shared/realtime-map-update";
import { UpdateGeoLocation } from "./shared/update-geo-location";
import { UpdateMap } from "./shared/update-map";

export interface IModalMoveFileProps {
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

  const MessageAddLocation = language.text("Voeg locatie toe", "Add location");
  const MessageUpdateLocation = language.text(
    "Werk locatie bij",
    "Update location"
  );
  const MessageViewLocation = language.text("Bekijk locatie", "View location");
  const MessageCancel = language.text("Annuleren", "Cancel");
  const MessageErrorGenericFail = new Language(settings.language).text(
    "Er is iets misgegaan met het updaten. Probeer het opnieuw",
    "Something went wrong with the update. Please try again"
  );
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
      UpdateMap(
        node,
        location,
        isFormEnabled,
        setLocation,
        setIsLocationUpdated,
        setMapState
      );
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function subHeader(): string {
    let message: string;
    if (isFormEnabled) {
      if (!latitude && !longitude) {
        message = MessageAddLocation;
      } else {
        message = MessageUpdateLocation;
      }
    } else {
      message = MessageViewLocation;
    }
    return message;
  }

  function updateButton(): React.JSX.Element {
    return isLocationUpdated ? (
      <button
        onClick={async () => {
          const model = await UpdateGeoLocation(
            parentDirectory,
            selectedSubPath,
            location,
            setError,
            setIsLoading,
            props.collections
          );
          if (model) {
            props.handleExit(model);
          }
        }}
        data-test="update-geo-location"
        className="btn btn--default"
      >
        {!latitude && !longitude ? MessageAddLocation : MessageUpdateLocation}
      </button>
    ) : (
      <button className="btn btn--default" disabled={true}>
        {/* disabled */}
        {!latitude && !longitude ? MessageAddLocation : MessageUpdateLocation}
      </button>
    );
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
              {MessageErrorGenericFail}
            </div>
          </div>
        ) : null}
        <div
          className="content-geo"
          data-test="content-geo"
          ref={mapReference}
        ></div>
        <div className="modal modal-button-bar">
          <button
            data-test="force-cancel"
            onClick={() => props.handleExit(null)}
            className="btn btn--info"
          >
            {MessageCancel}
          </button>
          {isFormEnabled ? updateButton() : null}
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
