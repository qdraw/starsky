import { act } from '@testing-library/react';
import { mount, shallow } from 'enzyme';
import L from 'leaflet';
import React from 'react';
import { IConnectionDefault } from '../../../interfaces/IConnectionDefault';
import * as FetchXml from '../../../shared/fetch-xml';
import * as CurrentLocationButton from '../../atoms/current-location-button/current-location-button';
import { CurrentLocationButtonPropTypes } from '../../atoms/current-location-button/current-location-button';
import DetailViewGpx from './detail-view-gpx';

describe("DetailViewGpx", () => {

  it("renders (without state component)", () => {
    shallow(<DetailViewGpx></DetailViewGpx>)
  });

  describe("with Context", () => {
    it("renders with example GPX (very short one)", async () => {
      var responseString = '<?xml version="1.0" encoding="UTF - 8" ?><gpx version="1.1"><trkpt lat="52" lon="13"></trkpt></gpx>';
      const xmlParser = new DOMParser();
      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200, data: xmlParser.parseFromString(responseString, 'text/xml')
      } as IConnectionDefault);
      var spyGet = jest.spyOn(FetchXml, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

      var polylineSpy = jest.spyOn(L, "polyline").mockImplementationOnce(() => {
        return {
          addTo: jest.fn()
        } as any;
      })

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement('div');
      (window as any).domNode = div;
      document.body.appendChild(div);
      var gpx = mount(<DetailViewGpx></DetailViewGpx>, { attachTo: (window as any).domNode });

      expect(polylineSpy).toBeCalledTimes(0);

      expect(spyGet).toBeCalled();

      act(() => {
        gpx.unmount();
      })
    });

    var responseString = `<?xml version="1.0" encoding="UTF - 8" ?>
      <gpx version="1.1">
        <trkpt lat="52" lon="13"></trkpt>
        <trkpt lat="52" lon="13"></trkpt>
        <trkpt lat="55" lon="13"></trkpt>
      </gpx>`;
    const xmlParser = new DOMParser();

    it("renders with example GPX", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200, data: xmlParser.parseFromString(responseString, 'text/xml')
      } as IConnectionDefault);
      var spyGet = jest.spyOn(FetchXml, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement('div');
      (window as any).domNode = div;
      document.body.appendChild(div);

      var spyMap = jest.spyOn(L, "map").mockImplementationOnce(() => {
        return {
          ...new MapMock("", {}) as any,
          dragging: { disable: jest.fn() },
          touchZoom: { disable: jest.fn() },
          doubleClickZoom: { disable: jest.fn() },
          scrollWheelZoom: { disable: jest.fn() },
          boxZoom: { disable: jest.fn() },
          keyboard: { disable: jest.fn() },
          tap: { disable: jest.fn() },
        };
      });

      var polylineSpy = jest.spyOn(L, "polyline").mockImplementationOnce(() => {
        return {
          addTo: jest.fn()
        } as any;
      })

      var gpx = mount(<DetailViewGpx></DetailViewGpx>, { attachTo: (window as any).domNode });

      // need to await before the maps are added
      await act(async () => {
        await gpx.find(".main--gpx").simulate("click");
      })

      expect(gpx.exists(".main--gpx")).toBeTruthy();

      expect(spyGet).toBeCalled();
      expect(spyMap).toBeCalled();
      expect(polylineSpy).toBeCalled();

      act(() => {
        gpx.unmount();
      });
    });

    it("zoom in", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200, data: xmlParser.parseFromString(responseString, 'text/xml')
      } as IConnectionDefault);
      var spyGet = jest.spyOn(FetchXml, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement('div');
      (window as any).domNode = div;
      document.body.appendChild(div);

      var zoomIn = jest.fn();
      var enable = jest.fn();

      var spyMap = jest.spyOn(L, "map").mockImplementationOnce(() => {
        return {
          ...new MapMock("", {}) as any,
          dragging: { disable: jest.fn(), enable },
          touchZoom: { disable: jest.fn() },
          doubleClickZoom: { disable: jest.fn() },
          scrollWheelZoom: { disable: jest.fn() },
          boxZoom: { disable: jest.fn() },
          keyboard: { disable: jest.fn() },
          tap: { disable: jest.fn() },
          zoomIn
        };
      });

      var polylineSpy = jest.spyOn(L, "polyline").mockImplementationOnce(() => {
        return {
          addTo: jest.fn()
        } as any;
      })

      var gpx = mount(<DetailViewGpx></DetailViewGpx>, { attachTo: (window as any).domNode });

      // need to await before the maps are added
      await gpx.find(".main--gpx").simulate("click");

      gpx.find('[data-test="zoom_in"]').simulate('click');

      expect(spyGet).toBeCalled();
      expect(spyMap).toBeCalled();
      expect(polylineSpy).toBeCalled();
      expect(zoomIn).toBeCalled();
      expect(enable).toBeCalled();

      act(() => {
        gpx.unmount();
      });
    });

    it("zoom out", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200, data: xmlParser.parseFromString(responseString, 'text/xml')
      } as IConnectionDefault);
      var spyGet = jest.spyOn(FetchXml, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement('div');
      (window as any).domNode = div;
      document.body.appendChild(div);

      var zoomOut = jest.fn();
      var enable = jest.fn();

      var spyMap = jest.spyOn(L, "map").mockImplementationOnce(() => {
        return {
          ...new MapMock("", {}) as any,
          dragging: { disable: jest.fn(), enable },
          touchZoom: { disable: jest.fn() },
          doubleClickZoom: { disable: jest.fn() },
          scrollWheelZoom: { disable: jest.fn() },
          boxZoom: { disable: jest.fn() },
          keyboard: { disable: jest.fn() },
          tap: { disable: jest.fn() },
          zoomOut
        };
      });

      var polylineSpy = jest.spyOn(L, "polyline").mockImplementationOnce(() => {
        return {
          addTo: jest.fn()
        } as any;
      })

      var gpx = mount(<DetailViewGpx></DetailViewGpx>, { attachTo: (window as any).domNode });

      // need to await before the maps are added
      await gpx.find(".main--gpx").simulate("click");

      gpx.find('[data-test="zoom_out"]').simulate('click');

      expect(spyGet).toBeCalled();
      expect(spyMap).toBeCalled();
      expect(polylineSpy).toBeCalled();
      expect(zoomOut).toBeCalled();
      expect(enable).toBeCalled();

      act(() => {
        gpx.unmount();
      });
    });

    it("zoom out", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200, data: xmlParser.parseFromString(responseString, 'text/xml')
      } as IConnectionDefault);
      jest.spyOn(FetchXml, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement('div');
      (window as any).domNode = div;
      document.body.appendChild(div);

      var zoomOut = jest.fn();
      var enable = jest.fn();
      var disable = jest.fn();

      jest.spyOn(L, "map").mockImplementationOnce(() => {
        return {
          ...new MapMock("", {}) as any,
          dragging: { disable, enable },
          touchZoom: { disable: jest.fn() },
          doubleClickZoom: { disable: jest.fn() },
          scrollWheelZoom: { disable: jest.fn() },
          boxZoom: { disable: jest.fn() },
          keyboard: { disable: jest.fn() },
          tap: { disable: jest.fn() },
          zoomOut
        };
      });

      jest.spyOn(L, "polyline").mockImplementationOnce(() => {
        return {
          addTo: jest.fn()
        } as any;
      })

      var gpx = mount(<DetailViewGpx></DetailViewGpx>, { attachTo: (window as any).domNode });

      // need to await before the maps are added
      await gpx.find(".main--gpx").simulate("click");

      // Enable first
      gpx.find('[data-test="lock"]').simulate('click');

      expect(enable).toBeCalled();

      // And disable afterwards
      gpx.find('[data-test="lock"]').simulate('click');

      expect(disable).toBeCalled();

      act(() => {
        gpx.unmount();
      });
    });

    it("current location", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> = Promise.resolve({
        statusCode: 200, data: xmlParser.parseFromString(responseString, 'text/xml')
      } as IConnectionDefault);
      jest.spyOn(FetchXml, 'default').mockImplementationOnce(() => mockGetIConnectionDefault);

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement('div');
      (window as any).domNode = div;
      document.body.appendChild(div);

      var setViewSpy = jest.fn();
      var lMapMock = () => {
        return {
          ...new MapMock("", {}) as any,
          dragging: { disable: jest.fn(), enable: jest.fn() },
          touchZoom: { disable: jest.fn() },
          doubleClickZoom: { disable: jest.fn() },
          scrollWheelZoom: { disable: jest.fn() },
          boxZoom: { disable: jest.fn() },
          keyboard: { disable: jest.fn() },
          tap: { disable: jest.fn() },
          setView: setViewSpy
        };
      };
      jest.spyOn(L, "map").mockImplementationOnce(lMapMock);

      jest.spyOn(L, "polyline").mockImplementationOnce(() => {
        return {
          addTo: jest.fn()
        } as any;
      })

      var locationButton = (input: CurrentLocationButtonPropTypes) => {
        return <button id="current-location" onClick={() => {
          if (!input.callback) return;
          input.callback({ longitude: 1, latitude: 1 } as Coordinates)
        }}></button>
      };
      var currentLocationButtonSpy = jest.spyOn(CurrentLocationButton, 'default')
        .mockImplementationOnce(locationButton)
        .mockImplementationOnce(locationButton)
        .mockImplementationOnce(locationButton)
        .mockImplementationOnce(locationButton);


      var gpx = mount(<DetailViewGpx></DetailViewGpx>, { attachTo: (window as any).domNode });

      // need to await before the maps are added
      await gpx.find(".main--gpx").simulate("click");

      await gpx.find("#current-location").simulate("click");

      expect(currentLocationButtonSpy).toBeCalled();
      expect(setViewSpy).toBeCalled();
      expect(setViewSpy).toBeCalledWith(new L.LatLng(1, 1), 15, { animate: true });

      act(() => {
        gpx.unmount();
      });
    });

  });

  const LeafletMock: any = jest.genMockFromModule('leaflet')

  class MapMock extends LeafletMock.Map {
    constructor(id: any, options: any = {}) {
      super();
      Object.assign(this, L.Evented.prototype)

      options = { ...L.Map.prototype.options, ...options }

      if (options.bounds) {
        this.fitBounds(options.bounds, options.boundsOptions)
      }

      if (options.maxBounds) {
        this.setMaxBounds(options.maxBounds)
      }

      if (options.center && options.zoom !== undefined) {
        this.setView(L.latLng(options.center), options.zoom)
      }
    }

    _limitZoom(zoom: any) {
      return Math.max(0, Math.min(0, zoom))
    }

    _resetView(center: any, zoom: any) {
    }

    fitBounds(bounds: any, options: any) {
    }

    getBounds() {
    }

    getCenter() {
    }

    getMaxZoom() {
    }

    getMinZoom() {
    }

    getZoom() {
    }

    setMaxBounds(bounds: any) {
    }

    setView(center: any, zoom: any) {
    }

    setZoom(zoom: any) {
      return this.setView(this.getCenter(), zoom)
    }
  }

});