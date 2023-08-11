import L from "leaflet";
import MarkerBlueSvg from "../../../style/images/fa-map-marker-blue.svg";
import MarkerShadowPng from "../../../style/images/marker-shadow.png";

export const blueIcon = L.icon({
  iconUrl: MarkerBlueSvg,
  shadowUrl: MarkerShadowPng,
  iconSize: [50, 50], // size of the icon
  shadowSize: [50, 50], // size of the shadow
  iconAnchor: [25, 50], // point of the icon which will correspond to marker's location
  shadowAnchor: [15, 55], // the same for the shadow
  popupAnchor: [0, -50] // point from which the popup should open relative to the iconAnchor
});