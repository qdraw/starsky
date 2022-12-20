import L from "leaflet";
import React, { useCallback, useEffect, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IGeoLocationModel } from "../../../interfaces/IGeoLocationModel";
import { Geo } from "../../../shared/geo";
import { Language } from "../../../shared/language";
import {
  tileLayerAttribution,
  tileLayerLocation
} from "../../../shared/tile-layer-location.const";
import MarkerBlueSvg from "../../../style/images/fa-map-marker-blue.svg";
import MarkerShadowPng from "../../../style/images/marker-shadow.png";
import FormControl from "../../atoms/form-control/form-control";
import Modal from "../../atoms/modal/modal";
import { updateGeoLocation } from "./update-geo-location";

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

const blueIcon = L.icon({
  iconUrl: MarkerBlueSvg,
  shadowUrl: MarkerShadowPng,
  iconSize: [50, 50], // size of the icon
  shadowSize: [50, 50], // size of the shadow
  iconAnchor: [25, 50], // point of the icon which will correspond to marker's location
  shadowAnchor: [15, 55], // the same for the shadow
  popupAnchor: [0, -50] // point from which the popup should open relative to the iconAnchor
});

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

export function addDefaultMarker(
  location: ILatLong,
  map: L.Map,
  isFormEnabled: boolean,
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>,
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>
): void {
  if (location.latitude && location.longitude) {
    const markerLocal = new L.Marker(
      {
        lat: location.latitude,
        lng: location.longitude
      },
      {
        draggable: isFormEnabled,
        icon: blueIcon
      }
    );
    markerLocal.on("dragend", (event) =>
      onDrag(event, setLocation, setIsLocationUpdated)
    );
    map.addLayer(markerLocal);
  }
}

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

function setMarker(
  map: L.Map,
  isFormEnabled: boolean,
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>,
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>,
  lat: number,
  lng: number
) {
  if (!isFormEnabled) {
    return;
  }

  map.eachLayer(function (layer) {
    if (layer instanceof L.Marker) {
      map.removeLayer(layer);
    }
  });

  const markerLocal = new L.Marker(
    { lat, lng },
    {
      draggable: true,
      icon: blueIcon
    }
  );

  markerLocal.on("dragend", (event) =>
    onDrag(event, setLocation, setIsLocationUpdated)
  );

  setLocation({
    latitude: latLongRound(lat),
    longitude: latLongRound(lng)
  });

  setIsLocationUpdated(true);
  map.addLayer(markerLocal);
}

export function addDefaultClickSetMarker(
  map: L.Map,
  isFormEnabled: boolean,
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>,
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>
) {
  map.on("click", function (event) {
    setMarker(
      map,
      isFormEnabled,
      setLocation,
      setIsLocationUpdated,
      event.latlng.lat,
      event.latlng.lng
    );
  });
}

function latLongRound(latitudeLong: number | undefined) {
  return !!latitudeLong ? Math.round(latitudeLong * 1000000) / 1000000 : 0;
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

  addDefaultMarker(
    location,
    map,
    isFormEnabled,
    setLocation,
    setIsLocationUpdated
  );

  addDefaultClickSetMarker(
    map,
    isFormEnabled,
    setLocation,
    setIsLocationUpdated
  );

  setMapState(map);
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

    setMarker(
      mapState,
      isFormEnabled,
      setLocation,
      setIsLocationUpdated,
      latitude,
      longitude
    );

    setIsLocationUpdated(false);
    mapState.panTo(new L.LatLng(latitude, longitude));

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
    return isFormEnabled
      ? !latitude && !longitude
        ? MessageAddLocation
        : MessageUpdateLocation
      : MessageViewLocation;
  }

  function updateButton(): JSX.Element {
    return isLocationUpdated ? (
      <button
        onClick={async () => {
          const model = await updateGeoLocation(
            parentDirectory,
            selectedSubPath,
            location,
            setError,
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
              className={"inline"}
              name={"lat"}
            >
              {location.latitude}
            </FormControl>{" "}
            <b>Longitude:</b>{" "}
            <FormControl
              contentEditable={false}
              className={"inline"}
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
