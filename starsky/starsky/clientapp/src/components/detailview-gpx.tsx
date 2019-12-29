import L from 'leaflet';
import React, { useEffect, useRef, useState } from 'react';
import useLocation from '../hooks/use-location';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import FetchXml from '../shared/fetch-xml';
import { LeafletEmptyImageUrlGridLayer } from '../shared/leaflet-modify-empty-image-url-gridlayer';
import { LeafletEmptyImageUrlTileLayer } from '../shared/leaflet-modify-empty-image-url-tilelayer';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';
import Preloader from './preloader';

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

    // create map
    var map = L.map(mapReference.current, {
      layers: [
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
          attribution:
            '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        }),
      ]
    });

    // map.dragging.disable();
    // map.touchZoom.disable();
    // map.doubleClickZoom.disable();
    // map.scrollWheelZoom.disable();
    // map.boxZoom.disable();
    // map.keyboard.disable();
    // if (map.tap) map.tap.disable();

    map.fitBounds(tracks);

    L.polyline(tracks, { color: '#455A64', fill: false }).addTo(map);
  }

  // Due a strict CSP policy the following line is not allowed ==> 
  // https://github.com/Leaflet/Leaflet/blob/e4b49000843687046cb127811d395394eb93e931/src/core/Util.js#L198
  L.GridLayer.include(new LeafletEmptyImageUrlGridLayer()._removeTile);
  L.GridLayer.include(new LeafletEmptyImageUrlGridLayer()._tileReady);
  L.TileLayer.include(new LeafletEmptyImageUrlTileLayer("")._abortLoading);

  // when having to gpx files next and you browse though it
  const mapReference = useRef<HTMLDivElement>(null);

  /** update only on intial load */
  useEffect(() => {
    setIsLoading(true);
    var filePathEncoded = new URLPath().encodeURI(new URLPath().getFilePath(history.location.search));
    FetchXml(new UrlQuery().UrlDownloadPhotoApi(filePathEncoded, false)).then((response) => {
      updateMap(response);
      setIsLoading(false);
    })
  }, [new URLPath().getFilePath(history.location.search)]);

  return (
    <>
      {isLoading ? <Preloader isDetailMenu={false} isOverlay={false} /> : ""}
      <div className={"main main--error main--gpx"} ref={mapReference} />
    </>
  );
};

export default DetailViewGpx;
