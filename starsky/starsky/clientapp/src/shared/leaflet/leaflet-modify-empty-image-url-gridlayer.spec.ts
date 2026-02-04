import L, { Coords, InternalTiles } from "leaflet";
import { LeafletEmptyImageUrlGridLayer } from "./leaflet-modify-empty-image-url-gridlayer";

describe("LeafletEmptyImageUrlGridLayer [leaflet-extension]", () => {
  it("remove tile form leaflet [extension]", () => {
    const gridlayer = new LeafletEmptyImageUrlGridLayer();

    const el = document.createElement("div");

    // mock a tile
    (gridlayer as unknown as { _tiles: InternalTiles })._tiles = {
      test: {
        coords: {} as Coords,
        current: true,
        el
      }
    } as InternalTiles;

    const fire = jest.fn();
    gridlayer.fire = fire;

    // to skip some check in leaflet TypeError: symbol is not a function
    Object.defineProperty(L.Browser, "androidStock", {
      value: true,
      writable: true
    });

    // this is normaly excuted by leaflet
    gridlayer._removeTile("test");

    expect((gridlayer as unknown as { _tiles: InternalTiles })._tiles).toStrictEqual({});
    expect(fire).toHaveBeenCalled();
  });

  it("tile ready form leaflet (_fadeAnimated disabled) [extension]", () => {
    const requestAnimFrameSpy = jest
      .spyOn(L.Util, "requestAnimFrame")
      .mockImplementationOnce(() => {
        return 0;
      });

    const exampleCoords = {
      x: 51,
      y: 10,
      z: 1
    } as Coords;
    const el = document.createElement("div") as unknown as HTMLElement;

    const gridlayer = new LeafletEmptyImageUrlGridLayer();

    (gridlayer as unknown as { _map: boolean })._map = true;
    // mock a tile
    (gridlayer as unknown as { _tiles: InternalTiles })._tiles = {
      "51:10:1": {
        coords: exampleCoords,
        current: true,
        el
      }
    } as InternalTiles;

    const _pruneTiles = jest.fn();
    (gridlayer as unknown as { _pruneTiles: () => void })._pruneTiles = _pruneTiles;

    (gridlayer as unknown as { _noTilesToLoad: () => boolean })._noTilesToLoad = () => {
      return true;
    };

    gridlayer._tileReady(exampleCoords, null, {
      getAttribute: jest.fn(),
      coords: exampleCoords,
      current: true,
      el
    });

    expect(_pruneTiles).toHaveBeenCalled();
    expect(requestAnimFrameSpy).toHaveBeenCalled();
  });

  it("tile ready form leaflet (_fadeAnimated enabled) [extension]", () => {
    const requestAnimFrameSpy = jest
      .spyOn(L.Util, "requestAnimFrame")
      .mockImplementationOnce(() => {
        return 0;
      });

    const timeoutSpy = jest.spyOn(global, "setTimeout");

    const exampleCoords = {
      x: 51,
      y: 10,
      z: 1
    } as Coords;
    const el = document.createElement("div") as unknown as HTMLElement;

    const gridlayer = new LeafletEmptyImageUrlGridLayer();

    (gridlayer as unknown as { _map: boolean })._map = true;

    // mock a tile
    (gridlayer as unknown as { _tiles: InternalTiles })._tiles = {
      "51:10:1": {
        coords: exampleCoords,
        current: true,
        el
      }
    } as InternalTiles;

    (gridlayer as unknown as { _map: { _fadeAnimated: boolean } })._map = {
      _fadeAnimated: true
    };

    (gridlayer as unknown as { _noTilesToLoad: () => boolean })._noTilesToLoad = () => {
      return true;
    };

    // with error enabled
    gridlayer._tileReady(exampleCoords, true, {
      getAttribute: jest.fn(),
      coords: exampleCoords,
      current: true,
      el
    });

    expect(requestAnimFrameSpy).toHaveBeenCalled();
    expect(timeoutSpy).toHaveBeenCalled();
  });

  it("should return early if _map is falsy", () => {
    const gridlayer = new LeafletEmptyImageUrlGridLayer();
    (gridlayer as unknown as { _map: boolean | undefined })._map = undefined;
    const coords = { x: 1, y: 2, z: 3 } as L.Coords;
    const tile = {
      getAttribute: jest.fn().mockReturnValue("not-empty-image.gif"),
      coords,
      current: true,
      el: document.createElement("div")
    };
    // Should return early, not throw
    expect(() => gridlayer._tileReady(coords, null, tile)).not.toThrow();
  });

  it("should return early if tile src is empty-image.gif", () => {
    const gridlayer = new LeafletEmptyImageUrlGridLayer();
    (gridlayer as unknown as { _map: boolean | undefined })._map = true;
    const coords = { x: 1, y: 2, z: 3 } as L.Coords;
    const tile = {
      getAttribute: jest.fn((attr) => (attr === "src" ? "empty-image.gif" : null)),
      coords,
      current: true,
      el: document.createElement("div")
    };
    // Should return early, not throw
    expect(() => gridlayer._tileReady(coords, null, tile)).not.toThrow();
  });

  it("should not return early if tile.getAttribute is undefined", () => {
    const gridlayer = new LeafletEmptyImageUrlGridLayer();
    (gridlayer as unknown as { _map: boolean | undefined })._map = true;
    const coords = { x: 1, y: 2, z: 3 } as L.Coords;
    const tile = {
      getAttribute: undefined,
      coords,
      current: true,
      el: document.createElement("div")
    };
    // Should not return early, should continue
    // To test, we need to mock _tileCoordsToKey and _tiles
    const key = "1:2:3";
    (gridlayer as unknown as { _tileCoordsToKey: (c: L.Coords) => string })._tileCoordsToKey = () =>
      key;
    (gridlayer as unknown as { _tiles: unknown })._tiles = {
      [key]: { ...tile }
    };
    (gridlayer as unknown as { _pruneTiles: () => void })._pruneTiles = jest.fn();
    (gridlayer as unknown as { _noTilesToLoad: () => boolean })._noTilesToLoad = () => false;
    expect(() => gridlayer._tileReady(coords, null, tile)).not.toThrow();
  });
});
