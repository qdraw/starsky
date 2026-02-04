import { ILatLong } from "../modal-geo";
import { LatLongRound } from "./lat-long-round";

export const OnDrag = function (
  dragEndEvent: L.DragEndEvent,
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>,
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>
) {
  const latlng = dragEndEvent.target.getLatLng();
  setLocation({
    latitude: LatLongRound(latlng.lat),
    longitude: LatLongRound(latlng.lng)
  });
  setIsLocationUpdated(true);
};
