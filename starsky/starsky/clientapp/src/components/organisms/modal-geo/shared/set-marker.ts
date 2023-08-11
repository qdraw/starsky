import L from "leaflet";
import { ILatLong, onDrag } from "../modal-geo";
import { blueIcon } from "./blue-icon";
import { latLongRound } from "./lat-long-round";

export function SetMarker(
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