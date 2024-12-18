import { render, screen } from "@testing-library/react";
import CurrentLocationButton from "./current-location-button";

describe("CurrentLocationButton", () => {
  it("renders", () => {
    render(<CurrentLocationButton />);
  });

  function globalNavigator() {
    return global as unknown as {
      navigator: {
        geolocation: {
          getCurrentPosition: (
            success: (position: { coords: { latitude: number; longitude: number } }) => void,
            error?: (error: Error) => void
          ) => void;
        };
      };
    };
  }

  describe("context", () => {
    it("no navigator.geolocation wrong_location", async () => {
      const component = render(<CurrentLocationButton />);
      const button = screen.getByRole("button");

      // need to await
      await button.click();

      const list = screen.getByRole("button").classList;
      expect(list).toContain("icon--wrong_location");

      component.unmount();
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

      globalNavigator().navigator.geolocation = mockGeolocation;

      const callback = jest.fn();
      const component = render(<CurrentLocationButton callback={callback} />);
      screen.getByRole("button").click();

      expect(callback).toHaveBeenCalled();
      expect(callback).toHaveBeenCalledWith({ latitude: 51.1, longitude: 45.3 });

      expect(screen.getByRole("button").classList).toContain("icon--location_on");

      component.unmount();
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
      globalNavigator().navigator.geolocation = mockGeolocation;

      const component = render(<CurrentLocationButton />);
      screen.getByRole("button").click();

      // no callback
      expect(screen.getByRole("button").classList).toContain("icon--location_on");

      component.unmount();
    });

    it("getCurrentPosition error", async () => {
      const mockGeolocation = {
        getCurrentPosition: jest.fn().mockImplementationOnce((_, error) => Promise.resolve(error()))
      };
      globalNavigator().navigator.geolocation = mockGeolocation;

      const callback = jest.fn();
      const component = render(<CurrentLocationButton callback={callback} />);

      // need to await here
      await screen.getByRole("button").click();

      expect(screen.getByRole("button").classList).toContain("icon--wrong_location");
      component.unmount();
    });
  });
});
