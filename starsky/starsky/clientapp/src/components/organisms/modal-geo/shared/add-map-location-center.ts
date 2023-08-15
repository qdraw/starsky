import L from "leaflet";
import { Geo } from "../../../../shared/geo";
import { ILatLong } from "../modal-geo";

export function AddMapLocationCenter(location: ILatLong): L.LatLng {
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
