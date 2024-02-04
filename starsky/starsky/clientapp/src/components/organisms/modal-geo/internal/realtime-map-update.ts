import L from "leaflet";
import { ILatLong } from "../modal-geo";
import { SetMarker } from "./set-marker";

export function RealtimeMapUpdate(
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
