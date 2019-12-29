import { Coords, InternalTiles } from 'leaflet';
import { LeafletEmptyImageUrlGridLayer } from './leaflet-modify-empty-image-url-gridlayer';

describe("LeafletEmptyImageUrlGridLayer", () => {
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

  it("tile ready form leaflet [extension]", () => {
    var gridlayer = new LeafletEmptyImageUrlGridLayer();

    var exampleCoords = {
      x: 51,
      y: 10
    } as Coords;

    gridlayer._tileReady(exampleCoords, null, "test");

  });

});