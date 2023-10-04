/* eslint-disable @typescript-eslint/no-explicit-any */
import { act, render, screen, waitFor } from "@testing-library/react";
import L from "leaflet";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { Coordinates } from "../../../shared/coordinates-position.types";
import * as FetchXml from "../../../shared/fetch-xml";
import * as CurrentLocationButton from "../../atoms/current-location-button/current-location-button";
import { CurrentLocationButtonPropTypes } from "../../atoms/current-location-button/current-location-button";
import DetailViewGpx from "./detail-view-gpx";

describe("DetailViewGpx", () => {
  const responseString =
    '<?xml version="1.0" encoding="UTF - 8" ?><gpx version="1.1"><trkpt lat="52" lon="13"></trkpt></gpx>';
  const xmlParser = new DOMParser();
  const mockGetIConnectionDefault: Promise<IConnectionDefault> =
    Promise.resolve({
      statusCode: 200,
      data: xmlParser.parseFromString(responseString, "text/xml")
    } as IConnectionDefault);

  it("renders (without state component)", () => {
    jest
      .spyOn(FetchXml, "default")
      .mockImplementationOnce(() => mockGetIConnectionDefault);
    const component = render(<DetailViewGpx></DetailViewGpx>);
    component.unmount();
  });

  describe("with Context", () => {
    it("renders with example GPX (very short one)", async () => {
      const spyGet = jest
        .spyOn(FetchXml, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      const polylineSpy = jest
        .spyOn(L, "polyline")
        .mockImplementationOnce(() => {
          return {
            addTo: jest.fn()
          } as any;
        });

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement("div");
      (window as any).domNode = div;
      document.body.appendChild(div);

      const gpx = render(<DetailViewGpx></DetailViewGpx>);

      expect(polylineSpy).toBeCalledTimes(0);

      expect(spyGet).toBeCalled();

      gpx.unmount();
      // act(() => {
      //   gpx.unmount();
      //   document.body.innerHTML = "";
      //   (window as any).domNode = null;
      // });
    });

    const responseString = `<gpx version="1.1">
        <trkpt lat="52" lon="13"></trkpt>
        <trkpt lat="52" lon="13"></trkpt>
        <trkpt lat="55" lon="13"></trkpt>
      </gpx>`;
    const xmlParser = new DOMParser();

    it("renders with example GPX", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
          statusCode: 200,
          data: xmlParser.parseFromString(responseString, "text/xml")
        } as IConnectionDefault);

      jest.spyOn(FetchXml, "default").mockReset();

      const spyGet = jest
        .spyOn(FetchXml, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement("div");
      (window as any).domNode = div;
      document.body.appendChild(div);

      const spyMap = jest.spyOn(L, "map").mockImplementationOnce(() => {
        return {
          ...(new MapMock("", {}) as any),
          dragging: { disable: jest.fn() },
          touchZoom: { disable: jest.fn() },
          doubleClickZoom: { disable: jest.fn() },
          scrollWheelZoom: { disable: jest.fn() },
          boxZoom: { disable: jest.fn() },
          keyboard: { disable: jest.fn() },
          tap: { disable: jest.fn() }
        };
      });

      const polylineSpy = jest
        .spyOn(L, "polyline")
        .mockImplementationOnce(() => {
          return {
            addTo: jest.fn()
          } as any;
        });

      // attachTo: (window as any).domNode
      const gpx = render(<DetailViewGpx></DetailViewGpx>);

      await waitFor(() => expect(spyGet).toBeCalled());

      expect(spyMap).toBeCalled();
      expect(polylineSpy).toBeCalled();

      gpx.unmount();
    });

    it("[gpx] zoom in", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
          statusCode: 200,
          data: xmlParser.parseFromString(responseString, "text/xml")
        } as IConnectionDefault);
      const spyGet = jest
        .spyOn(FetchXml, "default")
        .mockReset()
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      // // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      // const div = document.createElement("div");
      // (window as any).domNode = div;
      // document.body.appendChild(div);

      const zoomIn = jest.fn();
      const enable = jest.fn();

      const spyMap = jest.spyOn(L, "map").mockImplementationOnce(() => {
        return {
          ...(new MapMock("", {}) as any),
          dragging: { disable: jest.fn(), enable },
          touchZoom: { disable: jest.fn(), enable: jest.fn() },
          doubleClickZoom: { disable: jest.fn(), enable: jest.fn() },
          scrollWheelZoom: { disable: jest.fn() },
          boxZoom: { disable: jest.fn() },
          keyboard: { disable: jest.fn() },
          tap: { disable: jest.fn() },
          zoomIn
        };
      });

      const polylineSpy = jest
        .spyOn(L, "polyline")
        .mockImplementationOnce(() => {
          return {
            addTo: jest.fn()
          } as any;
        });

      const gpx = render(<DetailViewGpx></DetailViewGpx>);

      expect(spyGet).toBeCalled();
      expect(spyGet).toBeCalledTimes(1);

      // expect(spyMap).toBeCalled();
      await waitFor(() => expect(spyMap).toBeCalled());
      expect(polylineSpy).toBeCalled();

      const zoom_in = screen.queryByTestId("zoom_in");
      act(() => {
        zoom_in?.click();
      });

      expect(zoomIn).toBeCalled();
      expect(enable).toBeCalled();

      gpx.unmount();
      expect(spyGet).toBeCalledTimes(1);
    });

    it("[detail view gpx] - zoom out 1", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
          statusCode: 200,
          data: xmlParser.parseFromString(responseString, "text/xml")
        } as IConnectionDefault);
      const spyGet = jest
        .spyOn(FetchXml, "default")
        .mockReset()
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement("div");
      (window as any).domNode = div;
      document.body.appendChild(div);

      const zoomOut = jest.fn();
      const enable = jest.fn();

      const spyMap = jest.spyOn(L, "map").mockImplementationOnce(() => {
        return {
          ...(new MapMock("", {}) as any),
          dragging: { disable: jest.fn(), enable },
          touchZoom: { disable: jest.fn(), enable: jest.fn() },
          doubleClickZoom: { disable: jest.fn(), enable: jest.fn() },
          scrollWheelZoom: { disable: jest.fn() },
          boxZoom: { disable: jest.fn() },
          keyboard: { disable: jest.fn() },
          tap: { disable: jest.fn() },
          zoomOut
        };
      });

      const polylineSpy = jest
        .spyOn(L, "polyline")
        .mockImplementationOnce(() => {
          return {
            addTo: jest.fn()
          } as any;
        });

      const gpx = render(<DetailViewGpx></DetailViewGpx>);

      expect(spyGet).toBeCalled();
      expect(spyGet).toBeCalledTimes(1);

      await waitFor(() => expect(spyMap).toBeCalled());
      expect(polylineSpy).toBeCalled();

      const zoom_out = screen.queryByTestId("zoom_out");

      // add await
      await act(() => {
        zoom_out?.click();
      });

      expect(zoomOut).toBeCalled();
      expect(enable).toBeCalled();

      gpx.unmount();
      expect(spyGet).toBeCalledTimes(1);
    });

    it("zoom out 2", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
          statusCode: 200,
          data: xmlParser.parseFromString(responseString, "text/xml")
        } as IConnectionDefault);

      jest.spyOn(FetchXml, "default").mockClear();

      jest
        .spyOn(FetchXml, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault)
        .mockImplementationOnce(() => mockGetIConnectionDefault)
        .mockImplementationOnce(() => mockGetIConnectionDefault)
        .mockImplementationOnce(() => mockGetIConnectionDefault)
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      // const div = document.createElement("div");
      // (window as any).domNode = div;
      // document.body.appendChild(div);

      const zoomOut = jest.fn();
      const enable = jest.fn();
      const disable = jest.fn();

      const spyMap = jest.spyOn(L, "map").mockImplementationOnce(() => {
        return {
          ...(new MapMock("", {}) as any),
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
      });

      //attachTo: (window as any).domNode
      const gpx = render(<DetailViewGpx></DetailViewGpx>);

      // need to await before the maps are added
      await waitFor(() => expect(spyMap).toBeCalled());

      // Enable first
      const button = screen.queryByTestId("lock") as HTMLButtonElement;
      act(() => {
        button.click();
      });

      expect(enable).toBeCalled();

      // And disable afterwards
      const button2 = screen.queryByTestId("lock") as HTMLButtonElement;

      act(() => {
        button2.click();
      });

      expect(disable).toBeCalled();

      gpx.unmount();
    });

    it("current location", async () => {
      const mockGetIConnectionDefault: Promise<IConnectionDefault> =
        Promise.resolve({
          statusCode: 200,
          data: xmlParser.parseFromString(responseString, "text/xml")
        } as IConnectionDefault);

      jest
        .spyOn(FetchXml, "default")
        .mockImplementationOnce(() => mockGetIConnectionDefault);

      // https://stackoverflow.com/questions/43694975/jest-enzyme-using-mount-document-getelementbyid-returns-null-on-componen
      const div = document.createElement("div");
      (window as any).domNode = div;
      document.body.appendChild(div);

      const setViewSpy = jest.fn();
      const lMapMock = () => {
        return {
          ...(new MapMock("", {}) as any),
          dragging: { disable: jest.fn(), enable: jest.fn() },
          touchZoom: { disable: jest.fn(), enable: jest.fn() },
          doubleClickZoom: { disable: jest.fn(), enable: jest.fn() },
          scrollWheelZoom: { disable: jest.fn() },
          boxZoom: { disable: jest.fn() },
          keyboard: { disable: jest.fn() },
          tap: { disable: jest.fn() },
          setView: setViewSpy
        };
      };
      const spyMap = jest.spyOn(L, "map").mockImplementationOnce(lMapMock);

      jest.spyOn(L, "polyline").mockImplementationOnce(() => {
        return {
          addTo: jest.fn()
        } as any;
      });

      const locationButton = (input: CurrentLocationButtonPropTypes) => {
        return (
          <button
            id="current-location"
            data-test="current-location-button"
            onClick={() => {
              if (!input.callback) return;
              input.callback({ longitude: 1, latitude: 1 } as Coordinates);
            }}
          ></button>
        );
      };
      const currentLocationButtonSpy = jest
        .spyOn(CurrentLocationButton, "default")
        .mockImplementationOnce(locationButton)
        .mockImplementationOnce(locationButton)
        .mockImplementationOnce(locationButton)
        .mockImplementationOnce(locationButton);

      const gpx = render(<DetailViewGpx></DetailViewGpx>);

      // need to await before the maps are added
      await waitFor(() => expect(spyMap).toBeCalled());

      const button = screen.queryByTestId(
        "current-location-button"
      ) as HTMLButtonElement;
      expect(button).toBeTruthy();
      await button.click();

      expect(currentLocationButtonSpy).toBeCalled();
      expect(setViewSpy).toBeCalled();
      expect(setViewSpy).toBeCalledWith(new L.LatLng(1, 1), 15, {
        animate: true
      });

      act(() => {
        gpx.unmount();
      });
    });
  });

  const LeafletMock: any = jest.genMockFromModule("leaflet");

  class MapMock extends LeafletMock.Map {
    constructor(_id: any, options: any = {}) {
      super();
      Object.assign(this, L.Evented.prototype);

      options = { ...L.Map.prototype.options, ...options };

      if (options.bounds) {
        this.fitBounds(options.bounds, options.boundsOptions);
      }

      if (options.maxBounds) {
        this.setMaxBounds(options.maxBounds);
      }

      if (options.center && options.zoom !== undefined) {
        this.setView(L.latLng(options.center), options.zoom);
      }
    }

    _limitZoom(zoom: any) {
      return Math.max(0, Math.min(0, zoom));
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    _resetView(_center: any, _zoom: any) {}

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    fitBounds(_bounds: any, _options: any) {}

    getBounds() {}

    getCenter() {}

    getMaxZoom() {}

    getMinZoom() {}

    getZoom() {}

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    setMaxBounds(_bounds: any) {}

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    setView(_center: any, _zoom: any) {}

    setZoom(zoom: any) {
      return this.setView(this.getCenter(), zoom);
    }
  }
});
