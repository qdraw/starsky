import L, { Coords, InternalTiles } from "leaflet";
import { LeafletEmptyImageUrlTileLayer } from "./leaflet-modify-empty-image-url-tilelayer";

describe("LeafletEmptyImageUrlTileLayer [leaflet-extension]", () => {
  it("remove tile form leaflet [extension]", () => {
    // to skip some check in leaflet TypeError: symbol is not a function
    Object.defineProperty(L.Browser, "androidStock", {
      value: true,
      writable: true
    });

    const exampleCoords = {
      x: 51,
      y: 10,
      z: 1
    } as Coords;
    const el = document.createElement("div") as HTMLDivElement;

    const tileLayer = new LeafletEmptyImageUrlTileLayer("51:10:1") as unknown as {
      _tiles: InternalTiles;
      _abortLoading: () => void;
    };

    // mock a tile
    tileLayer._tiles = {
      "51:10:1": {
        coords: exampleCoords,
        current: true,
        el
      }
    } as InternalTiles;

    tileLayer._abortLoading();

    // no content anymore
    expect(tileLayer._tiles).toStrictEqual({});
  });
});
