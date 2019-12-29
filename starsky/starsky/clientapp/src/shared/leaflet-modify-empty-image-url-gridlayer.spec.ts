import { LeafletEmptyImageUrlGridLayer } from './leaflet-modify-empty-image-url-gridlayer';

describe("LeafletEmptyImageUrlGridLayer", () => {
  it("isInForm", () => {


    var gridlayer = new LeafletEmptyImageUrlGridLayer();

    (gridlayer as any)._tiles = {
      "test": ""
    }

    var fire = jest.fn();
    gridlayer.fire = fire

    gridlayer._removeTile("test");
    expect(fire).toBeCalled();

  });
});