import L from "leaflet";
import React, { useCallback, useEffect, useState } from "react";
import { IGeoLocationModel } from "../../../interfaces/IGeoLocationModel";
import { Geo } from "../../../shared/geo";
import {
  tileLayerAttribution,
  tileLayerLocation
} from "../../../shared/tile-layer-location.const";

import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";
import FormControl from "../../atoms/form-control/form-control";
import Modal from "../../atoms/modal/modal";
import Preloader from "../../atoms/preloader/preloader";
import { AddDefaultMarker } from "./shared/add-default-marker";
import { latLongRound } from "./shared/lat-long-round";
import { SetMarker } from "./shared/set-marker";
import { UpdateGeoLocation } from "./shared/update-geo-location";

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



export function getZoom(location: ILatLong): number {
  let zoom = 12;
  if (location.latitude && location.longitude) {
    zoom = 15;
  }
  return zoom;
}

export const onDrag = function (
  dragEndEvent: L.DragEndEvent,
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>,
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>
) {
  const latlng = dragEndEvent.target.getLatLng();
  setLocation({
    latitude: latLongRound(latlng.lat),
    longitude: latLongRound(latlng.lng)
  });
  setIsLocationUpdated(true);
};



export function addMapLocationCenter(location: ILatLong): L.LatLng {
  let mapLocationCenter = L.latLng(52.375, 4.9);
  if (
    location.latitude &&
    location.longitude &&
    new Geo().Validate(location.latitude, location.longitude)
  ) {
    mapLocationCenter = L.latLng(location.latitude, location.longitude);
  }
  return mapLocationCenter;
}

export function addMap(
  mapLocationCenter: L.LatLng,
  node: HTMLDivElement,
  zoom: number
): L.Map {
  // Leaflet maps
  const map = L.map(node, {
    center: mapLocationCenter,
    zoom,
    layers: [
      L.tileLayer(tileLayerLocation, {
        attribution: tileLayerAttribution
      })
    ]
  });
  return map;
}


export function addDefaultClickSetMarker(
  map: L.Map,
  isFormEnabled: boolean,
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>,
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>
) {
  map.on("click", function (event) {
    SetMarker(
      map,
      isFormEnabled,
      setLocation,
      setIsLocationUpdated,
      event.latlng.lat,
      event.latlng.lng
    );
  });
}

function updateMap(
  node: HTMLDivElement,
  location: ILatLong,
  isFormEnabled: boolean,
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>,
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>,
  setMapState: React.Dispatch<React.SetStateAction<L.Map | null>>
) {
  const zoom = getZoom(location);

  const mapLocationCenter = addMapLocationCenter(location);

  const map = addMap(mapLocationCenter, node, zoom);

  isFormEnabled = AddDefaultMarker(
    location,
    map,
    isFormEnabled,
    setLocation,
    setIsLocationUpdated
  );
  console.log("isFormEnabled: " + isFormEnabled);
  

  addDefaultClickSetMarker(
    map,
    isFormEnabled,
    setLocation,
    setIsLocationUpdated
  );

  setMapState(map);
}

export function realtimeMapUpdate(
  mapState: L.Map,
  isFormEnabled: boolean,
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>,
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>,
  latitude: number,
  longitude: number
) {
  SetMarker(
    mapState,
    isFormEnabled,
    setLocation,
    setIsLocationUpdated,
    latitude,
    longitude
  );

  setIsLocationUpdated(false);
  mapState.panTo(new L.LatLng(latitude, longitude));
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
    latitude: latLongRound(latitude),
    longitude: latLongRound(longitude)
  });

  useEffect(() => {
    if (mapState === null || !latitude || !longitude) {
      return;
    }

    if (location.latitude === latitude && location.longitude === longitude) {
      return;
    }

    realtimeMapUpdate(
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
      updateMap(
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
