import L, { Coords, InternalTiles } from "leaflet";
import { LeafletEmptyImageUrlGridLayer } from "./leaflet-modify-empty-image-url-gridlayer";

describe("LeafletEmptyImageUrlGridLayer [leaflet-extension]", () => {
  it("remove tile form leaflet [extension]", () => {
    const gridlayer = new LeafletEmptyImageUrlGridLayer();

    const el = document.createElement("div");

    // mock a tile
    (gridlayer as any)._tiles = {
      test: {
        coords: {} as Coords,
        current: true,
        el
      }
    } as InternalTiles;

    const fire = jest.fn();
    gridlayer.fire = fire;

    // to skip some check in leaflet TypeError: symbol is not a function
    Object.defineProperty(L.Browser, 'androidStock', {
      value: true,
      writable: true
    });
    
    // this is normaly excuted by leaflet
    gridlayer._removeTile("test");

    expect((gridlayer as any)._tiles).toStrictEqual({});
    expect(fire).toBeCalled();
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
    const el = document.createElement("div") as any;

    const gridlayer = new LeafletEmptyImageUrlGridLayer();

    (gridlayer as any)._map = true;
    // mock a tile
    (gridlayer as any)._tiles = {
      "51:10:1": {
        coords: exampleCoords,
        current: true,
        el
      }
    } as InternalTiles;

    const _pruneTiles = jest.fn();
    (gridlayer as any)._pruneTiles = _pruneTiles;

    (gridlayer as any)._noTilesToLoad = () => {
      return true;
    };

    gridlayer._tileReady(exampleCoords, null, el);

    expect(_pruneTiles).toBeCalled();
    expect(requestAnimFrameSpy).toBeCalled();
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
    const el = document.createElement("div") as any;

    const gridlayer = new LeafletEmptyImageUrlGridLayer();

    (gridlayer as any)._map = true;

    // mock a tile
    (gridlayer as any)._tiles = {
      "51:10:1": {
        coords: exampleCoords,
        current: true,
        el
      }
    } as InternalTiles;

    (gridlayer as any)._map = {
      _fadeAnimated: true
    };

    (gridlayer as any)._noTilesToLoad = () => {
      return true;
    };

    // with error enabled
    gridlayer._tileReady(exampleCoords, true, el);

    expect(requestAnimFrameSpy).toBeCalled();
    expect(timeoutSpy).toBeCalled();
  });
});
