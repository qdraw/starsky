import { ILatLong } from "../modal-geo";
import { AddDefaultClickSetMarker } from "./add-default-click-set-marker";
import { AddDefaultMarker } from "./add-default-marker";
import { AddMap } from "./add-map";
import { AddMapLocationCenter } from "./add-map-location-center";
import { GetZoom } from "./get-zoom";

export function UpdateMap(
  node: HTMLDivElement,
  location: ILatLong,
  isFormEnabled: boolean,
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>,
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>,
  setMapState: React.Dispatch<React.SetStateAction<L.Map | null>>
) {
  const zoom = GetZoom(location);

  const mapLocationCenter = AddMapLocationCenter(location);

  const map = AddMap(mapLocationCenter, node, zoom);

  isFormEnabled = AddDefaultMarker(
    location,
    map,
    isFormEnabled,
    setLocation,
    setIsLocationUpdated
  );

  AddDefaultClickSetMarker(
    map,
    isFormEnabled,
    setLocation,
    setIsLocationUpdated
  );

  setMapState(map);
}
