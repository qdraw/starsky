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
});
