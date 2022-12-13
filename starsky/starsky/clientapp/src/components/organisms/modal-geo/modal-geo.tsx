import L from "leaflet";
import React, { useCallback, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IGeoLocationModel } from "../../../interfaces/IGeoLocationModel";
import FetchGet from "../../../shared/fetch-get";
import FetchPost from "../../../shared/fetch-post";
import { Geo } from "../../../shared/geo";
import { Language } from "../../../shared/language";
import {
  tileLayerAttribution,
  tileLayerLocation
} from "../../../shared/tile-layer-location.const";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import MarkerBlueSvg from "../../../style/images/fa-map-marker-blue.svg";
import MarkerShadowPng from "../../../style/images/marker-shadow.png";
import Modal from "../../atoms/modal/modal";

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

interface ILatLong {
  latitude: number;
  longitude: number;
}

async function updateGeoLocation(
  parentDirectory: string,
  selectedSubPath: string,
  location: ILatLong | null,
  setError: React.Dispatch<React.SetStateAction<boolean>>,
  collections?: boolean
): Promise<IGeoLocationModel | null> {
  if (!location?.latitude || !location?.longitude) {
    return Promise.resolve(null);
  }

  const bodyParams = new URLPath().ObjectToSearchParams({
    collections,
    f: parentDirectory + "/" + selectedSubPath,
    append: false
  });
  bodyParams.append("latitude", location.latitude.toString());
  bodyParams.append("longitude", location.longitude.toString());

  let model = {} as IGeoLocationModel;
  try {
    const reverseGeoCodeResult = await FetchGet(
      new UrlQuery().UrlReverseLookup(
        location.latitude.toString(),
        location.longitude.toString()
      )
    );
    if (reverseGeoCodeResult.statusCode === 200) {
      model = reverseGeoCodeResult.data;
      bodyParams.append("locationCity", model.locationCity);
      bodyParams.append("locationCountry", model.locationCountry);
      bodyParams.append("locationCountryCode", model.locationCountryCode);
      bodyParams.append("locationState", model.locationState);
    }
    console.log(reverseGeoCodeResult.statusCode);
  } catch (error) {}

  console.log(bodyParams.toString());

  try {
    const updateResult = await FetchPost(
      new UrlQuery().UrlUpdateApi(),
      bodyParams.toString()
    );
    if (updateResult.statusCode !== 200) {
      setError(true);
      return Promise.resolve(null);
    }
  } catch (error) {
    setError(true);
    return Promise.resolve(null);
  }

  return Promise.resolve(model);
}

const ModalGeo: React.FunctionComponent<IModalMoveFileProps> = (props) => {
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
    latitude: !!props.latitude ? props.latitude : 0,
    longitude: !!props.longitude ? props.longitude : 0
  });
  const [isLocationUpdated, setIsLocationUpdated] = useState<boolean>(false);

  const blueIcon = L.icon({
    iconUrl: MarkerBlueSvg,
    shadowUrl: MarkerShadowPng,
    iconSize: [50, 50], // size of the icon
    shadowSize: [50, 50], // size of the shadow
    iconAnchor: [25, 50], // point of the icon which will correspond to marker's location
    shadowAnchor: [15, 55], // the same for the shadow
    popupAnchor: [0, -50] // point from which the popup should open relative to the iconAnchor
  });

  const onDrag = function (dragEndEvent: L.DragEndEvent) {
    const latlng = dragEndEvent.target.getLatLng();
    setLocation({
      latitude: latlng.lat,
      longitude: latlng.lng
    });
    setIsLocationUpdated(true);
  };

  const mapReference = useCallback((node: HTMLDivElement | null) => {
    if (node !== null && mapState === null) {
      let mapLocationCenter = L.latLng(52.375, 4.9);
      if (
        location.latitude &&
        location.longitude &&
        new Geo().Validate(location.latitude, location.longitude)
      ) {
        mapLocationCenter = L.latLng(location.latitude, location.longitude);
      }

      let zoom = 12;
      if (location.latitude && location.longitude) {
        zoom = 15;
      }

      const map = L.map(node, {
        center: mapLocationCenter,
        zoom,
        layers: [
          L.tileLayer(tileLayerLocation, {
            attribution: tileLayerAttribution
          })
        ]
      });

      if (location.latitude && location.longitude) {
        const markerLocal = new L.Marker(
          {
            lat: location.latitude,
            lng: location.longitude
          },
          {
            draggable: props.isFormEnabled,
            icon: blueIcon
          }
        );
        markerLocal.on("dragend", onDrag);
        map.addLayer(markerLocal);
      }

      map.on("click", function (event) {
        if (!props.isFormEnabled) {
          return;
        }

        map.eachLayer(function (layer) {
          if (layer instanceof L.Marker) {
            map.removeLayer(layer);
          }
        });

        const markerLocal = new L.Marker(event.latlng, {
          draggable: true,
          icon: blueIcon
        });

        markerLocal.on("dragend", onDrag);

        setLocation({
          latitude: event.latlng.lat,
          longitude: event.latlng.lng
        });

        setIsLocationUpdated(true);
        map.addLayer(markerLocal);
      });

      setMapState(map);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <Modal
      id="move-geo"
      className="modal-bg-large"
      isOpen={props.isOpen}
      handleExit={() => props.handleExit}
    >
      <div className="content" data-test="modal-geo">
        <div className="modal content--subheader">
          {props.isFormEnabled
            ? !props.latitude && !props.longitude
              ? MessageAddLocation
              : MessageUpdateLocation
            : MessageViewLocation}
        </div>
        {error ? (
          <div className="modal modal-button-bar-error">
            <div data-test="login-error" className="content--error-true">
              {MessageErrorGenericFail}
            </div>
          </div>
        ) : null}
        <div className="content-geo" ref={mapReference}></div>
        <div className="modal modal-button-bar">
          <button
            data-test="force-cancel"
            onClick={() => props.handleExit(null)}
            className="btn btn--info"
          >
            {MessageCancel}
          </button>
          {props.isFormEnabled ? (
            isLocationUpdated ? (
              <button
                onClick={async () => {
                  const model = await updateGeoLocation(
                    props.parentDirectory,
                    props.selectedSubPath,
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
                {!props.latitude && !props.longitude
                  ? MessageAddLocation
                  : MessageUpdateLocation}
              </button>
            ) : (
              <button className="btn btn--default" disabled={true}>
                {/* disabled */}
                {!props.latitude && !props.longitude
                  ? MessageAddLocation
                  : MessageUpdateLocation}
              </button>
            )
          ) : null}
        </div>
      </div>
    </Modal>
  );
};

export default ModalGeo;
