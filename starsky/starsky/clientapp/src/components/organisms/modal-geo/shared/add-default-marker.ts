import L from "leaflet";
import { ILatLong, onDrag } from "../modal-geo";
import { blueIcon } from "./blue-icon";

export function AddDefaultMarker(
  location: ILatLong,
  map: L.Map,
  isFormEnabled: boolean,
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>,
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>
): boolean {
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
  return isFormEnabled;
}