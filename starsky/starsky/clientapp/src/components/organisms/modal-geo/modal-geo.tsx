import L from "leaflet";
import React, { useCallback, useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";
import {
  tileLayerAttribution,
  tileLayerLocation
} from "../../../shared/tile-layer-location.const";
import MarkerBlueSvg from "../../../style/images/fa-map-marker-blue.svg";
import MarkerShadowPng from "../../../style/images/marker-shadow.png";
import Modal from "../../atoms/modal/modal";

interface IModalMoveFileProps {
  isOpen: boolean;
  handleExit: Function;
  selectedSubPath: string;
  parentDirectory: string;
  latitude?: number;
  longitude?: number;
}

interface ILatLong {
  latitude: number;
  longitude: number;
}

// /**
//  * To update the archive
//  */
// function pushUpdate() {

//   // // loading + update button
//   // setIsLoading(true);

//   const bodyParams = new URLPath().ObjectToSearchParams(update);
//   if (bodyParams.toString().length === 0) return;

//   // const subPaths = new URLPath().MergeSelectFileIndexItem(
//   //   select,
//   //   state.fileIndexItems
//   // );
//   // if (!subPaths) return;
//   // const selectParams = new URLPath().ArrayToCommaSeperatedStringOneParent(
//   //   subPaths,
//   //   ""
//   // );

//   if (selectParams.length === 0) return;
//   bodyParams.append("f", selectParams);

//   FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams.toString())
//     .then((anyData) => {
//       const result = new CastToInterface().InfoFileIndexArray(anyData.data);
//       result.forEach((element) => {
//         if (element.status === IExifStatus.ReadOnly)
//           setIsError(MessageErrorReadOnly);
//         if (element.status === IExifStatus.NotFoundSourceMissing)
//           setIsError(MessageErrorNotFoundSourceMissing);
//         if (
//           element.status === IExifStatus.Ok ||
//           element.status === IExifStatus.Deleted
//         ) {
//           dispatch({
//             type: "update",
//             ...element,
//             select: [element.fileName]
//           });
//         }
//       });

//       // loading + update button
//       setIsLoading(false);
//       setInputEnabled(true);
//       ClearSearchCache(history.location.search);
//       // undo error message when success
//       if (isError === MessageErrorGenericFail) {
//         setIsError("");
//       }
//     })
//     .catch(() => {
//       setIsError(MessageErrorGenericFail);
//       // loading + update button
//       setIsLoading(false);
//       setInputEnabled(true);
//     });
// }

const ModalGeo: React.FunctionComponent<IModalMoveFileProps> = (props) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageUpdateLocation = language.text(
    "Werk locatie bij",
    "Update location"
  );
  const MessageCancel = language.text("Annuleren", "Cancel");

  const [mapState, setMapState] = useState<L.Map | null>(null);
  const [location, setLocation] = useState<ILatLong | null>({
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
    var latlng = dragEndEvent.target.getLatLng();
    setLocation({
      latitude: latlng.lat,
      longitude: latlng.lng
    });
    setIsLocationUpdated(true);
  };

  const mapReference = useCallback((node: HTMLDivElement | null) => {
    if (node !== null && mapState === null) {
      let mapLocationCenter = L.latLng(52.375, 4.9);
      if (props.latitude && props.longitude) {
        mapLocationCenter = L.latLng(props.latitude, props.longitude);
      }

      let zoom = 12;
      if (props.latitude && props.longitude) {
        zoom = 15;
      }

      const map = L.map(node, {
        center: mapLocationCenter!,
        zoom,
        layers: [
          L.tileLayer(tileLayerLocation, {
            attribution: tileLayerAttribution
          })
        ]
      });

      if (props.latitude && props.longitude) {
        const markerLocal = new L.Marker(
          {
            lat: props.latitude,
            lng: props.longitude
          },
          {
            draggable: true,
            icon: blueIcon
          }
        );
        markerLocal.on("dragend", onDrag);
        map.addLayer(markerLocal);
      }

      map.on("click", function (event) {
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
      handleExit={() => {
        props.handleExit();
      }}
    >
      <div className="content" data-test="modal-geo">
        <div className="modal content--subheader">{MessageUpdateLocation}</div>
        <div className="content-geo" ref={mapReference}></div>
        <div className="modal modal-button-bar">
          <button
            data-test="force-cancel"
            onClick={() => props.handleExit()}
            className="btn btn--info"
          >
            {MessageCancel}
          </button>
          {isLocationUpdated ? (
            <button
              onClick={() => {
                // pushUpdate();
                props.handleExit();
              }}
              data-test="update-geo-location"
              className="btn btn--default"
            >
              {MessageUpdateLocation}
            </button>
          ) : (
            <button className="btn btn--default" disabled={true}>
              {MessageUpdateLocation}
            </button>
          )}
        </div>
      </div>
    </Modal>
  );
};

export default ModalGeo;
