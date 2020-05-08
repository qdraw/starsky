import L from 'leaflet';
import React, { useEffect, useRef, useState } from 'react';
import useLocation from '../../../hooks/use-location';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import FetchXml from '../../../shared/fetch-xml';
import { Geo } from '../../../shared/geo';
import { LeafletEmptyImageUrlGridLayer } from '../../../shared/leaflet-modify-empty-image-url-gridlayer';
import { LeafletEmptyImageUrlTileLayer } from '../../../shared/leaflet-modify-empty-image-url-tilelayer';
import { URLPath } from '../../../shared/url-path';
import { UrlQuery } from '../../../shared/url-query';
import MarkerBlueSvg from '../../../style/images/fa-map-marker-blue.svg';
import MarkerShadowPng from '../../../style/images/marker-shadow.png';
import Preloader from '../../atoms/preloader/preloader';

const DetailViewGpx: React.FC = () => {
  var history = useLocation();

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  function updateMap(response: IConnectionDefault) {
    if (!response.data) return;
    if (!mapReference.current) return;

    // reset leaflet first
    mapReference.current.innerHTML = "";
    var container = L.DomUtil.get(mapReference.current);
    if (container != null) {
      (container as any)._leaflet_id = null;
    }

    var tracks: any[] = [];
    var tracksNodeList: NodeListOf<Element> = (response.data as XMLDocument).querySelectorAll('trkpt');

    Array.from(tracksNodeList).forEach(element => {
      tracks.push([element.getAttribute('lat'), element.getAttribute('lon')]);
    });

    // to avoid short inputs
    if (!tracks || tracks.length <= 2) return;

    // create map
    var map = L.map(mapReference.current, {
      layers: [
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
          attribution:
            '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        }),
      ]
    });

    map.dragging.disable();
    map.touchZoom.disable();
    map.doubleClickZoom.disable();
    map.scrollWheelZoom.disable();
    map.boxZoom.disable();
    map.keyboard.disable();
    if (map.tap) map.tap.disable();

    map.fitBounds(tracks);

    var blueIcon = L.icon({
      iconUrl: MarkerBlueSvg,
      shadowUrl: MarkerShadowPng,
      iconSize: [50, 50], // size of the icon
      shadowSize: [50, 50], // size of the shadow
      iconAnchor: [25, 50], // point of the icon which will correspond to marker's location
      shadowAnchor: [15, 55],  // the same for the shadow
      popupAnchor: [0, -50] // point from which the popup should open relative to the iconAnchor
    });

    var firstTrack = tracks[0];
    var lastTrack = tracks[tracks.length - 1];

    L.marker(tracks[0], { icon: blueIcon }).addTo(map);

    if (new Geo().Distance(firstTrack, lastTrack) >= 500) {
      L.marker(lastTrack, { icon: blueIcon }).addTo(map);
    }

    L.polyline(tracks, { color: '#455A64', fill: false }).addTo(map);
  }

  // Due a strict CSP policy the following line is not allowed ==> 
  // https://github.com/Leaflet/Leaflet/blob/e4b49000843687046cb127811d395394eb93e931/src/core/Util.js#L198
  new LeafletEmptyImageUrlGridLayer()
  new LeafletEmptyImageUrlTileLayer("51:12:1");

  // when having to gpx files next and you browse though it
  const mapReference = useRef<HTMLDivElement>(null);

  /** update to make useEffect simpler te read */
  const [filePathEncoded, setFilePathEncoded] = useState(new URLPath().encodeURI(new URLPath().getFilePath(history.location.search)));
  useEffect(() => {
    setFilePathEncoded(new URLPath().encodeURI(new URLPath().getFilePath(history.location.search)))
  }, [history.location.search]);

  /** update only on intial load */
  useEffect(() => {
    setIsLoading(true);
    FetchXml(new UrlQuery().UrlDownloadPhotoApi(filePathEncoded, false)).then((response) => {
      updateMap(response);
      setIsLoading(false);
    })
  }, [filePathEncoded]);

  return (
    <>
      {isLoading ? <Preloader isDetailMenu={false} isOverlay={false} /> : ""}
      <div className={"main main--error main--gpx"} ref={mapReference} />
    </>
  );
};

export default DetailViewGpx;
