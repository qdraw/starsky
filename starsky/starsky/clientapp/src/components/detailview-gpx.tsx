import L from 'leaflet';
import React, { useEffect, useRef } from 'react';
import useLocation from '../hooks/use-location';
import { IConnectionDefault } from '../interfaces/IConnectionDefault';
import FetchXml from '../shared/fetch-xml';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';

const DetailViewGpx: React.FC = () => {
  var history = useLocation();

  function updateMap(response: IConnectionDefault) {
    if (!response.data) return;
    if (!mapReference.current) return;

    // reset leaflet first
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
        L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
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

    L.polygon(tracks, { color: 'red', fill: false }).addTo(map);
  }

  // when having to gpx files next and you browse though it
  const mapReference = useRef<HTMLDivElement>(null);

  /** update only on intial load */
  useEffect(() => {
    var filePathEncoded = new URLPath().encodeURI(new URLPath().getFilePath(history.location.search));
    FetchXml(new UrlQuery().UrlDownloadPhotoApi(filePathEncoded, false)).then((response) => {
      updateMap(response)
    })
  }, [new URLPath().getFilePath(history.location.search)]);

  return (
    <div className={"main main--error main--gpx"} ref={mapReference} />
  );
};

export default DetailViewGpx;
