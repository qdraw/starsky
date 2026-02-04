import { ILatLong } from "../modal-geo";

export function GetZoom(location: ILatLong): number {
  let zoom = 12;
  if (location.latitude && location.longitude) {
    zoom = 15;
  }
  return zoom;
}
