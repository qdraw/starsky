import L from "leaflet";
import React, { useEffect, useRef, useState } from "react";
import useLocation from "../../../hooks/use-location/use-location";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { Coordinates } from "../../../shared/coordinates-position.types";
import FetchXml from "../../../shared/fetch/fetch-xml";
import { Geo } from "../../../shared/geo";
import { LeafletEmptyImageUrlGridLayer } from "../../../shared/leaflet/leaflet-modify-empty-image-url-gridlayer";
import { LeafletEmptyImageUrlTileLayer } from "../../../shared/leaflet/leaflet-modify-empty-image-url-tilelayer";
import { TileLayerAttribution, TileLayerLocation } from "../../../shared/tile-layer-location.const";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import MarkerBlueSvg from "../../../style/images/fa-map-marker-blue.svg";
import MarkerShadowPng from "../../../style/images/marker-shadow.png";
import CurrentLocationButton from "../../atoms/current-location-button/current-location-button";
import Preloader from "../../atoms/preloader/preloader";

const DetailViewGpx: React.FC = () => {
  const history = useLocation();

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  const [mapState, setMapState] = useState<L.Map>();
  const [isMapLocked, setIsMapLocked] = useState(true);

  function updateMap(response: IConnectionDefault) {
    if (!response.data) return;
    if (!mapReference.current) return;

    // reset leaflet first
    mapReference.current.innerHTML = "";
    const container = L.DomUtil.get(mapReference.current);
    if (container != null) {
      (container as unknown as { _leaflet_id: null })._leaflet_id = null;
    }

    const tracks: [number, number][] = [];
    const tracksNodeList: NodeListOf<Element> = (response.data as XMLDocument).querySelectorAll(
      "trkpt"
    );

    for (const element of Array.from(tracksNodeList)) {
      tracks.push([
        Number.parseFloat(element.getAttribute("lat") as string),
        Number.parseFloat(element.getAttribute("lon") as string)
      ]);
    }

    // to avoid short inputs
    if (!tracks || tracks.length <= 2) return;

    // create map
    const map = L.map(mapReference.current, {
      layers: [
        L.tileLayer(TileLayerLocation, {
          attribution: TileLayerAttribution
        })
      ]
    });

    map.dragging.disable();
    map.touchZoom.disable();
    map.doubleClickZoom.disable();
    map.scrollWheelZoom.disable();
    map.boxZoom.disable();
    map.keyboard.disable();
    if (map.tapHold) map.tapHold.disable();

    map.fitBounds(tracks);

    const blueIcon = L.icon({
      iconUrl: MarkerBlueSvg,
      shadowUrl: MarkerShadowPng,
      iconSize: [50, 50], // size of the icon
      shadowSize: [50, 50], // size of the shadow
      iconAnchor: [25, 50], // point of the icon which will correspond to marker's location
      shadowAnchor: [15, 55], // the same for the shadow
      popupAnchor: [0, -50] // point from which the popup should open relative to the iconAnchor
    });

    const firstTrack = tracks[0];
    const lastTrack = tracks[tracks.length - 1];

    L.marker(tracks[0], { title: "gpx", icon: blueIcon }).addTo(map);

    if (new Geo().Distance(firstTrack, lastTrack) >= 500) {
      L.marker(lastTrack, { icon: blueIcon }).addTo(map);
    }

    L.polyline(tracks, { color: "#455A64", fill: false }).addTo(map);
    setMapState(map);
  }

  // Due a strict CSP policy the following line is not allowed ==>
  // https://github.com/Leaflet/Leaflet/blob/e4b49000843687046cb127811d395394eb93e931/src/core/Util.js#L198
  new LeafletEmptyImageUrlGridLayer().init();
  new LeafletEmptyImageUrlTileLayer("51:12:1").init();

  // when having to gpx files next and you browse though it
  const mapReference = useRef<HTMLDivElement>(null);

  /** update to make useEffect simpler te read */
  const [filePathEncoded, setFilePathEncoded] = useState(
    new URLPath().encodeURI(new URLPath().getFilePath(history.location.search))
  );
  useEffect(() => {
    setFilePathEncoded(new URLPath().encodeURI(new URLPath().getFilePath(history.location.search)));
  }, [history.location.search]);

  /** update only on initial load */
  useEffect(() => {
    setIsLoading(true);
    FetchXml(new UrlQuery().UrlDownloadPhotoApi(filePathEncoded, false, true)).then((response) => {
      updateMap(response);
      setIsLoading(false);
    });
  }, [filePathEncoded]);

  function unLockLockToggle() {
    if (!mapState) return;
    if (isMapLocked) {
      mapState.dragging.enable();
    } else {
      mapState.dragging.disable();
    }
    setIsMapLocked(!isMapLocked);
  }

  function disableLock() {
    if (!mapState) return;
    mapState.dragging.enable();
    mapState.doubleClickZoom.enable();
    mapState.touchZoom.enable();
    setIsMapLocked(false);
  }

  function zoomIn() {
    if (!mapState) return;
    mapState.zoomIn();
    disableLock();
  }

  function zoomOut() {
    if (!mapState) return;
    mapState.zoomOut();
    disableLock();
  }

  function changeLocation(coords: Coordinates) {
    if (!mapState) return;
    mapState.setView(new L.LatLng(coords.latitude, coords.longitude), 15, {
      animate: true
    });
    disableLock();
  }

  return (
    <>
      {isLoading ? <Preloader isWhite={false} isOverlay={false} /> : ""}
      <div className="main main--error main--gpx" ref={mapReference} />
      <div className="gpx-controls">
        <div className="gpx-controls--button">
          <button
            data-test="lock"
            className={isMapLocked ? "icon icon--lock" : "icon icon--lock_open"}
            onClick={unLockLockToggle}
          >
            {isMapLocked ? "Unlock" : "Lock"}
          </button>
        </div>
        <div className="gpx-controls--button">
          <button data-test="zoom_in" className="icon icon--zoom_in" onClick={zoomIn}>
            Zoom in
          </button>
        </div>
        <div className="gpx-controls--button">
          <button data-test="zoom_out" className="icon icon--zoom_out" onClick={zoomOut}>
            Zoom out
          </button>
        </div>
        <div className="gpx-controls--button">
          <CurrentLocationButton callback={changeLocation}></CurrentLocationButton>
        </div>
      </div>
    </>
  );
};

export default DetailViewGpx;
