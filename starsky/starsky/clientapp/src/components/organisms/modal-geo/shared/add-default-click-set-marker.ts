import { ILatLong } from "../modal-geo";
import { SetMarker } from "./set-marker";

export function AddDefaultClickSetMarker(
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
