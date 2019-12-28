import L from 'leaflet';
import React, { useEffect } from 'react';
import useLocation from '../hooks/use-location';
import FetchXml from '../shared/fetch-xml';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';

const DetailViewGpx: React.FC = () => {
  var history = useLocation();

  useEffect(() => {
    var filePathEncoded = new URLPath().encodeURI(new URLPath().getFilePath(history.location.search));

    FetchXml(new UrlQuery().UrlDownloadPhotoApi(filePathEncoded, false)).then((response) => {
      if (!response.data) return;

      var tracks: any[] = [];
      var tracksNodeList: NodeListOf<Element> = (response.data as XMLDocument).querySelectorAll('trkpt');

      Array.from(tracksNodeList).forEach(element => {
        tracks.push([element.getAttribute('lat'), element.getAttribute('lon')]);
      });

      // create map
      var map = L.map('map', {
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
    })
  }, []);

  return (
    <div className="main main--error main--gpx" id="map" />
  );
};

export default DetailViewGpx;
