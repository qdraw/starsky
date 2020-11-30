import L, { Coords, InternalTiles } from 'leaflet';
import { LeafletEmptyImageUrlGridLayer } from './leaflet-modify-empty-image-url-gridlayer';

describe("LeafletEmptyImageUrlGridLayer [leaflet-extension]", () => {
  it("remove tile form leaflet [extension]", () => {

    var gridlayer = new LeafletEmptyImageUrlGridLayer();

    var el = document.createElement('div');

    // mock a tile
    (gridlayer as any)._tiles = {
      "test": {
        coords: {} as Coords,
        current: true,
        el,
      }
    } as InternalTiles

    var fire = jest.fn();
    gridlayer.fire = fire

    // this is normaly excuted by leaflet
    gridlayer._removeTile("test");

    expect((gridlayer as any)._tiles).toStrictEqual({})
    expect(fire).toBeCalled();
  });

  it("tile ready form leaflet (_fadeAnimated disabled) [extension]", () => {

    var requestAnimFrameSpy = jest.spyOn(L.Util, 'requestAnimFrame').mockImplementationOnce(() => { return 0 });

    var exampleCoords = {
      x: 51,
      y: 10,
      z: 1
    } as Coords;
    var el = document.createElement('div') as any;

    var gridlayer = new LeafletEmptyImageUrlGridLayer();

    (gridlayer as any)._map = true;
    // mock a tile
    (gridlayer as any)._tiles = {
      "51:10:1": {
        coords: exampleCoords,
        current: true,
        el,
      }
    } as InternalTiles

    var _pruneTiles = jest.fn();
    (gridlayer as any)._pruneTiles = _pruneTiles;

    (gridlayer as any)._noTilesToLoad = () => { return true };

    gridlayer._tileReady(exampleCoords, null, el);


    expect(_pruneTiles).toBeCalled();
    expect(requestAnimFrameSpy).toBeCalled();

  });


  it("tile ready form leaflet (_fadeAnimated enabled) [extension]", () => {

    var requestAnimFrameSpy = jest.spyOn(L.Util, 'requestAnimFrame').mockImplementationOnce(() => { return 0 });

    var timeoutSpy = jest.spyOn(global, 'setTimeout');

    var exampleCoords = {
      x: 51,
      y: 10,
      z: 1
    } as Coords;
    var el = document.createElement('div') as any;

    var gridlayer = new LeafletEmptyImageUrlGridLayer();

    (gridlayer as any)._map = true;

    // mock a tile
    (gridlayer as any)._tiles = {
      "51:10:1": {
        coords: exampleCoords,
        current: true,
        el,
      }
    } as InternalTiles


    (gridlayer as any)._map = {
      _fadeAnimated: true
    };

    (gridlayer as any)._noTilesToLoad = () => { return true };

    // with error enabled
    gridlayer._tileReady(exampleCoords, true, el);


    expect(requestAnimFrameSpy).toBeCalled();
    expect(timeoutSpy).toBeCalled();


  });

});