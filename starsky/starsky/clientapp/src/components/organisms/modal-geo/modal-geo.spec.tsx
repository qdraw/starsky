import { render, screen, waitFor } from "@testing-library/react";
import L, { LatLng } from "leaflet";
import { IGeoLocationModel } from "../../../interfaces/IGeoLocationModel";
import * as Modal from "../../atoms/modal/modal";

import ModalGeo, {
  addDefaultClickSetMarker,
  addDefaultMarker,
  getZoom,
  ILatLong,
  onDrag
} from "./modal-geo";
import * as updateGeoLocation from "./update-geo-location";

describe("ModalGeo", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});
  });

  it("renders", () => {
    render(
      <ModalGeo
        parentDirectory="/"
        selectedSubPath="/test.jpg"
        isOpen={true}
        handleExit={() => {}}
        latitude={51}
        longitude={3}
        isFormEnabled={false}
      ></ModalGeo>
    );
  });

  describe("getZoom", () => {
    it("getZoom none", () => {
      const result = getZoom({} as ILatLong);
      expect(result).toBe(12);
    });

    it("getZoom with location", () => {
      const result = getZoom({ latitude: 10, longitude: 15 } as ILatLong);
      expect(result).toBe(15);
    });
  });

  describe("addDefaultMarker", () => {
    it("no location so not called", () => {
      const map = {
        addLayer: jest.fn()
      } as unknown as L.Map;
      addDefaultMarker({} as ILatLong, map, true, jest.fn(), jest.fn());

      expect(map.addLayer).toBeCalledTimes(0);
    });

    it("location so called", () => {
      const map = {
        addLayer: jest.fn()
      } as unknown as L.Map;
      addDefaultMarker(
        { latitude: 10, longitude: 15 } as ILatLong,
        map,
        true,
        jest.fn(),
        jest.fn()
      );

      expect(map.addLayer).toBeCalledTimes(1);
    });
  });

  describe("onDrag", () => {
    it("should update setter", () => {
      const setLocationSpy = jest.fn();
      onDrag(
        {
          target: {
            getLatLng: () => {
              return {
                lat: 2,
                lng: 3
              };
            }
          }
        } as L.DragEndEvent,
        setLocationSpy,
        jest.fn()
      );

      expect(setLocationSpy).toBeCalledTimes(1);
      expect(setLocationSpy).toBeCalledWith({
        latitude: 2,
        longitude: 3
      });
    });
  });

  describe("addDefaultClickSetMarker", () => {
    it("should remove other layers and add new one", () => {
      const map = {
        on: (name: string, fn: Function) => {
          fn({ latlng: {} });
        },
        eachLayer: (fn: Function) => {
          fn(new L.Marker(new LatLng(0, 0)));
          fn({});
        },
        addLayer: jest.fn(),
        removeLayer: jest.fn()
      } as unknown as L.Map;
      addDefaultClickSetMarker(map, true, jest.fn(), jest.fn());

      expect(map.addLayer).toBeCalledTimes(1);
      expect(map.removeLayer).toBeCalledTimes(1);
    });

    it("should not add layers due readonly", () => {
      const map = {
        on: (name: string, fn: Function) => {
          fn({ latlng: {} });
        },
        eachLayer: (fn: Function) => {
          fn(new L.Marker(new LatLng(0, 0)));
          fn({});
        },
        addLayer: jest.fn(),
        removeLayer: jest.fn()
      } as unknown as L.Map;
      addDefaultClickSetMarker(map, false, jest.fn(), jest.fn());

      expect(map.addLayer).toBeCalledTimes(0);
      expect(map.removeLayer).toBeCalledTimes(0);
    });
  });

  describe("ModalGeo", () => {
    it("button should not be there when no update", async () => {
      const updateSpy = jest
        .spyOn(updateGeoLocation, "updateGeoLocation")
        .mockImplementationOnce(() => {
          return Promise.resolve(null);
        });
      const handleExitSpy = jest.fn();
      const modal = render(
        <ModalGeo
          parentDirectory="/"
          selectedSubPath="/test.jpg"
          isOpen={true}
          handleExit={handleExitSpy}
          latitude={51}
          longitude={3}
          isFormEnabled={true}
        ></ModalGeo>
      );

      expect(screen.queryByTestId("update-geo-location")).toBeNull();

      expect(updateSpy).toBeCalledTimes(0);

      expect(handleExitSpy).toBeCalledTimes(0);
      modal.unmount();
    });

    it("click on update button with edit - failed api", async () => {
      const updateSpy = jest
        .spyOn(updateGeoLocation, "updateGeoLocation")
        .mockImplementationOnce(() => {
          return Promise.resolve(null);
        });
      const handleExitSpy = jest.fn();
      const modal = render(
        <ModalGeo
          parentDirectory="/"
          selectedSubPath="/test.jpg"
          isOpen={true}
          handleExit={handleExitSpy}
          latitude={51}
          longitude={3}
          isFormEnabled={true}
        ></ModalGeo>
      );

      (await screen.findByTestId("content-geo")).click();

      const button = await screen.findByTestId("update-geo-location");
      button.click();

      expect(updateSpy).toBeCalled();
      expect(updateSpy).toBeCalledWith(
        "/",
        "/test.jpg",
        {
          latitude: 51.00001,
          longitude: 2.999997
        },
        expect.any(Function),
        undefined
      );
      expect(handleExitSpy).toBeCalledTimes(0);
      modal.unmount();
    });

    it("click on update button with edit - success api", async () => {
      const updateSpy = jest
        .spyOn(updateGeoLocation, "updateGeoLocation")
        .mockImplementationOnce(() => {
          return Promise.resolve({
            locationCity: "t"
          } as IGeoLocationModel);
        });
      const handleExitSpy = jest.fn();
      const modal = render(
        <ModalGeo
          parentDirectory="/"
          selectedSubPath="/test.jpg"
          isOpen={true}
          handleExit={handleExitSpy}
          latitude={51}
          longitude={3}
          isFormEnabled={true}
        ></ModalGeo>
      );

      (await screen.findByTestId("content-geo")).click();

      const button = await screen.findByTestId("update-geo-location");
      button.click();

      expect(updateSpy).toBeCalled();
      expect(updateSpy).toBeCalledWith(
        "/",
        "/test.jpg",
        {
          latitude: 51.00001,
          longitude: 2.999997
        },
        expect.any(Function),
        undefined
      );

      await waitFor(() => expect(handleExitSpy).toBeCalledTimes(1));
      expect(handleExitSpy).toBeCalledWith({ locationCity: "t" });
      modal.unmount();
    });

    it("press cancel button", async () => {
      const handleExitSpy = jest.fn();
      const modal = render(
        <ModalGeo
          parentDirectory="/"
          selectedSubPath="/test.jpg"
          isOpen={true}
          handleExit={handleExitSpy}
          latitude={51}
          longitude={3}
          isFormEnabled={true}
        ></ModalGeo>
      );

      (await screen.findByTestId("force-cancel")).click();

      expect(handleExitSpy).toBeCalledTimes(1);
      modal.unmount();
    });

    it("press cancel1 button", async () => {
      const handleExitSpy = jest.fn();
      const modal = render(
        <ModalGeo
          parentDirectory="/"
          selectedSubPath="/test.jpg"
          isOpen={true}
          handleExit={handleExitSpy}
          latitude={51}
          longitude={3}
          isFormEnabled={true}
        ></ModalGeo>
      );

      (await screen.findByTestId("force-cancel")).click();

      expect(handleExitSpy).toBeCalledTimes(1);
      modal.unmount();
    });

    it("test if handleExit is called", () => {
      // callback
      // simulate if a user press on close
      // use as ==> import * as Modal from './modal';
      jest.spyOn(Modal, "default").mockImplementationOnce((props) => {
        props.handleExit();
        return <>{props.children}</>;
      });

      const handleExitSpy = jest.fn();

      const modal = render(
        <ModalGeo
          parentDirectory="/"
          selectedSubPath="/test.jpg"
          isOpen={true}
          handleExit={handleExitSpy}
          latitude={51}
          longitude={3}
          isFormEnabled={true}
        ></ModalGeo>
      );

      expect(handleExitSpy).toBeCalled();

      // and clean afterwards
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

      modal.unmount();
    });
  });
});