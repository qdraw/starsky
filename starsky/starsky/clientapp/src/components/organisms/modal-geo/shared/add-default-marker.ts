import L from "leaflet";
import { ILatLong } from "../modal-geo";
import { blueIcon } from "./blue-icon";
import { OnDrag } from "./on-drag";

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
      OnDrag(event, setLocation, setIsLocationUpdated)
    );
    map.addLayer(markerLocal);
  }
  return isFormEnabled;
}
