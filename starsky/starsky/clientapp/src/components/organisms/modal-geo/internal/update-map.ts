import { Language } from "../../../../shared/language";
import { ILatLong } from "../modal-geo";
import { AddContextMenu, ILocalization } from "./add-context-menu";
import { AddDefaultClickSetMarker } from "./add-default-click-set-marker";
import { AddDefaultMarker } from "./add-default-marker";
import { AddMap } from "./add-map";
import { AddMapLocationCenter } from "./add-map-location-center";
import { GetZoom } from "./get-zoom";

interface IUpdateMapOptions {
  node: HTMLDivElement;
  location: ILatLong;
  isFormEnabled: boolean;
  setLocation: React.Dispatch<React.SetStateAction<ILatLong>>;
  setIsLocationUpdated: React.Dispatch<React.SetStateAction<boolean>>;
  setMapState: React.Dispatch<React.SetStateAction<L.Map | null>>;
  language: Language;
  localization: ILocalization;
  setNotificationStatus: React.Dispatch<React.SetStateAction<string | null>>;
}

export function UpdateMap({
  node,
  location,
  isFormEnabled,
  setLocation,
  setIsLocationUpdated,
  setMapState,
  language,
  localization,
  setNotificationStatus
}: IUpdateMapOptions) {
  const zoom = GetZoom(location);

  const mapLocationCenter = AddMapLocationCenter(location);

  const map = AddMap(mapLocationCenter, node, zoom);

  isFormEnabled = AddDefaultMarker(location, map, isFormEnabled, setLocation, setIsLocationUpdated);

  AddDefaultClickSetMarker(map, isFormEnabled, setLocation, setIsLocationUpdated);

  AddContextMenu({ map, setNotificationStatus, language, localization });

  setMapState(map);
}
