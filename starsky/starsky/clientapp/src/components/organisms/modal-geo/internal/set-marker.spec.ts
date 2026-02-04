import L from "leaflet";
import { SetMarker } from "./set-marker";

jest.mock("./on-drag", () => ({
  OnDrag: jest.fn()
}));

describe("SetMarker", () => {
  it("adds a draggable marker to the map and sets the location state", () => {
    const map = new L.Map(document.createElement("div"));
    const setLocation = jest.fn();
    const setIsLocationUpdated = jest.fn();
    const lat = 37.123456789;
    const lng = -122.987654321;
    SetMarker(map, true, setLocation, setIsLocationUpdated, lat, lng);
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-ignore
    expect(map.hasLayer(map._layers[Object.keys(map._layers)[0]])).toBe(true);
    expect(setLocation).toHaveBeenCalledWith({
      latitude: 37.123457,
      longitude: -122.987654
    });
    expect(setIsLocationUpdated).toHaveBeenCalledWith(true);
  });
});
