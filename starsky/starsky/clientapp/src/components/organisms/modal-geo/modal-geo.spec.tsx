/* eslint-disable @typescript-eslint/ban-types */
import { fireEvent, render, screen } from "@testing-library/react";
import L, { LatLng } from "leaflet";
import * as Modal from "../../atoms/modal/modal";
import { AddDefaultClickSetMarker } from "./internal/add-default-click-set-marker";
import * as AddDefaultMarker from "./internal/add-default-marker";
import { GetZoom } from "./internal/get-zoom";
import { OnDrag } from "./internal/on-drag";
import { RealtimeMapUpdate } from "./internal/realtime-map-update";
import * as UpdateButton from "./internal/update-button";
import * as updateGeoLocation from "./internal/update-geo-location";
import ModalGeo, { ILatLong } from "./modal-geo";

describe("ModalGeo", () => {
  beforeEach(() => {
    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

    jest.spyOn(L, "map").mockImplementationOnce(() => {
      return {
        addLayer: jest.fn(),
        on: jest.fn()
      } as any;
    });
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
      const result = GetZoom({} as ILatLong);
      expect(result).toBe(12);
    });

    it("getZoom with location", () => {
      const result = GetZoom({ latitude: 10, longitude: 15 } as ILatLong);
      expect(result).toBe(15);
    });
  });

  describe("addDefaultMarker", () => {
    it("no location so not called", () => {
      const map = {
        addLayer: jest.fn()
      } as unknown as L.Map;
      AddDefaultMarker.AddDefaultMarker({} as ILatLong, map, true, jest.fn(), jest.fn());

      expect(map.addLayer).toHaveBeenCalledTimes(0);
    });

    it("location so called", () => {
      const map = {
        addLayer: jest.fn()
      } as unknown as L.Map;
      AddDefaultMarker.AddDefaultMarker(
        { latitude: 10, longitude: 15 } as ILatLong,
        map,
        true,
        jest.fn(),
        jest.fn()
      );

      expect(map.addLayer).toHaveBeenCalledTimes(1);
    });
  });

  describe("onDrag", () => {
    it("should update setter", () => {
      const setLocationSpy = jest.fn();
      OnDrag(
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

      expect(setLocationSpy).toHaveBeenCalledTimes(1);
      expect(setLocationSpy).toHaveBeenCalledWith({
        latitude: 2,
        longitude: 3
      });
    });
  });

  describe("addDefaultClickSetMarker", () => {
    it("should remove other layers and add new one", () => {
      const map = {
        on: (_name: string, fn: Function) => {
          fn({ latlng: { lat: 1, lng: 1 } });
        },
        eachLayer: (fn: Function) => {
          fn(new L.Marker(new LatLng(0, 0)));
          fn({});
        },
        addLayer: jest.fn(),
        removeLayer: jest.fn()
      } as unknown as L.Map;
      AddDefaultClickSetMarker(map, true, jest.fn(), jest.fn());

      expect(map.addLayer).toHaveBeenCalledTimes(1);
      expect(map.removeLayer).toHaveBeenCalledTimes(1);
    });

    it("should not add layers due readonly", () => {
      const map = {
        on: (_name: string, fn: Function) => {
          fn({ latlng: {} });
        },
        eachLayer: (fn: Function) => {
          fn(new L.Marker(new LatLng(0, 0)));
          fn({});
        },
        addLayer: jest.fn(),
        removeLayer: jest.fn()
      } as unknown as L.Map;
      AddDefaultClickSetMarker(map, false, jest.fn(), jest.fn());

      expect(map.addLayer).toHaveBeenCalledTimes(0);
      expect(map.removeLayer).toHaveBeenCalledTimes(0);
    });
  });

  describe("realtimeMapUpdate", () => {
    it("should trigger pan to", () => {
      const panToSpy = jest.fn();
      const map = {
        on: (_name: string, fn: Function) => {
          fn({ latlng: {} });
        },
        eachLayer: (fn: Function) => {
          fn(new L.Marker(new LatLng(0, 0)));
          fn({});
        },
        panTo: panToSpy,
        addLayer: jest.fn(),
        removeLayer: jest.fn()
      } as unknown as L.Map;

      RealtimeMapUpdate(map, false, jest.fn(), jest.fn(), 1, 1);

      expect(map.addLayer).toHaveBeenCalledTimes(0);
      expect(map.removeLayer).toHaveBeenCalledTimes(0);
      expect(panToSpy).toHaveBeenCalledTimes(1);
      expect(panToSpy).toHaveBeenCalledWith({ lat: 1, lng: 1 });
    });
  });

  describe("ModalGeo", () => {
    it("button should not be there when no update", async () => {
      const updateSpy = jest
        .spyOn(updateGeoLocation, "UpdateGeoLocation")
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

      expect(updateSpy).toHaveBeenCalledTimes(0);

      expect(handleExitSpy).toHaveBeenCalledTimes(0);
      modal.unmount();
    });

    it("check if form is disabled it hides the button", async () => {
      const updateButtonSpy = jest
        .spyOn(UpdateButton, "UpdateButton")
        .mockImplementationOnce(() => {
          return <div>test</div>;
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
          isFormEnabled={false}
        ></ModalGeo>
      );

      expect(updateButtonSpy).toHaveBeenCalledTimes(0);

      modal.unmount();
    });

    it("check if form is enabled it shows the button", () => {
      const updateButtonSpy = jest
        .spyOn(UpdateButton, "UpdateButton")
        .mockImplementationOnce(() => {
          return <div>test</div>;
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

      expect(updateButtonSpy).toHaveBeenCalledTimes(2);

      expect(updateButtonSpy).toHaveBeenLastCalledWith(
        {
          handleExit: expect.any(Function),
          isLocationUpdated: false,
          location: { latitude: 51, longitude: 3 },
          parentDirectory: "/",
          propsCollections: undefined,
          selectedSubPath: "/test.jpg",
          setError: expect.any(Function),
          setIsLoading: expect.any(Function)
        },
        {}
      );

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

      expect(handleExitSpy).toHaveBeenCalledTimes(1);
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

      expect(handleExitSpy).toHaveBeenCalledTimes(1);
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

      // suppress error once
      jest.spyOn(console, "error").mockImplementationOnce(() => {});

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

      expect(handleExitSpy).toHaveBeenCalled();

      // and clean afterwards
      jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

      modal.unmount();
    });
  });

  describe("ModalGeo", () => {
    const props = {
      isOpen: true,
      isFormEnabled: true,
      handleExit: jest.fn(),
      selectedSubPath: "path/to/sub",
      parentDirectory: "path/to/parent",
      latitude: 37.123456789,
      longitude: -122.987654321,
      collections: true
    };

    it("displays the correct subheader message", () => {
      render(<ModalGeo {...props} />);
      const subheader = screen.getByTestId("force-cancel");
      expect(subheader).toBeDefined();
    });

    it("displays the correct latitude and longitude values", () => {
      const component = render(<ModalGeo {...props} />);
      const latitude = screen.getByTestId("modal-latitude");
      const longitude = screen.getByTestId("modal-longitude");
      expect(latitude.textContent).toEqual("37.123457");
      expect(longitude.textContent).toEqual("-122.987654");
      component.unmount();
    });

    it("calls handleExit with null when cancel button is clicked", () => {
      const component = render(<ModalGeo {...props} />);
      const cancelButton = screen.getByTestId("force-cancel");
      fireEvent.click(cancelButton);
      expect(props.handleExit).toHaveBeenCalledWith(null);
      component.unmount();
    });
  });
});
