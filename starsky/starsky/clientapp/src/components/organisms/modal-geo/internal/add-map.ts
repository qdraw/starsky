import L from "leaflet";
import {
  TileLayerAttribution,
  TileLayerLocation
} from "../../../../shared/tile-layer-location.const";

export function AddMap(mapLocationCenter: L.LatLng, node: HTMLDivElement, zoom: number): L.Map {
  // Leaflet maps
  const map = L.map(node, {
    center: mapLocationCenter,
    zoom,
    layers: [
      L.tileLayer(TileLayerLocation, {
        attribution: TileLayerAttribution
      })
    ]
  });
  return map;
}
