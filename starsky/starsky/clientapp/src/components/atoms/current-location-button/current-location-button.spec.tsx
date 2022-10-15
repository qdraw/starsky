import { render } from "@testing-library/react";
import React from "react";
import CurrentLocationButton from "./current-location-button";

describe("CurrentLocationButton", () => {
  it("renders", () => {
    render(<CurrentLocationButton />);
  });

  describe("context", () => {
    it("no navigator.geolocation wrong_location", async () => {
      const component = render(<CurrentLocationButton />);
      const button = component.getByRole("button");

      // need to await
      await button.click();

      const list = component.getByRole("button").classList;
      expect(list).toContain("icon--wrong_location");
    });

    it("getCurrentPosition success", () => {
      const mockGeolocation = {
        getCurrentPosition: jest.fn().mockImplementationOnce((success) =>
          Promise.resolve(
            success({
              coords: {
                latitude: 51.1,
                longitude: 45.3
              }
            })
          )
        )
      };
      (global as any).navigator.geolocation = mockGeolocation;

      const callback = jest.fn();
      const component = render(<CurrentLocationButton callback={callback} />);
      component.getByRole("button").click();

      expect(callback).toBeCalled();
      expect(callback).toBeCalledWith({ latitude: 51.1, longitude: 45.3 });

      expect(component.getByRole("button").classList).toContain(
        "icon--location_on"
      );
    });

    it("getCurrentPosition success no callback", () => {
      const mockGeolocation = {
        getCurrentPosition: jest.fn().mockImplementationOnce((success) =>
          Promise.resolve(
            success({
              coords: {
                latitude: 51.1,
                longitude: 45.3
              }
            })
          )
        )
      };
      (global as any).navigator.geolocation = mockGeolocation;

      const component = render(<CurrentLocationButton />);
      component.getByRole("button").click();

      // no callback
      expect(component.getByRole("button").classList).toContain(
        "icon--location_on"
      );
    });

    it("getCurrentPosition error", async () => {
      const mockGeolocation = {
        getCurrentPosition: jest
          .fn()
          .mockImplementationOnce((_, error) => Promise.resolve(error()))
      };
      (global as any).navigator.geolocation = mockGeolocation;

      const callback = jest.fn();
      const component = render(<CurrentLocationButton callback={callback} />);

      // need to await here
      await component.getByRole("button").click();

      expect(component.getByRole("button").classList).toContain(
        "icon--wrong_location"
      );
    });
  });
});
