import L, { Coords, GridLayer } from 'leaflet';
import EmptyImage from '../style/images/empty-image.gif';
// use the: IMAGE_INLINE_SIZE_LIMIT=1 due the fact that data: are not supported by the CSP


export class LeafletEmptyImageUrlGridLayer extends GridLayer {

  /**
   * Include Strict CSP policy for GridLayer
   * You need Grid and Tile Layer
   */
  constructor() {
    super();
    L.GridLayer.include({ _removeTile: this._removeTile });
    L.GridLayer.include({ _tileReady: this._tileReady });
  }

  public _removeTile(key: string) {
    var tile = this._tiles[key];
    if (!tile) { return; }

    // Cancels any pending http requests associated with the tile
    // unless we're on Android's stock browser,
    // see https://github.com/Leaflet/Leaflet/issues/137
    if (!L.Browser.androidStock) {
      tile.el.setAttribute('src', EmptyImage); // Replace emptyImageUrl
    }
    L.DomUtil.remove(tile.el);

    delete this._tiles[key];

    // @event tileunload: TileEvent
    // Fired when a tile is removed (e.g. when a tile goes off the screen).
    this.fire('tileunload', {
      tile: tile.el,
      coords: (this as any)._keyToTileCoords(key)
    });
  }

  /**
   * When Tile is ready, this is fired by leaflet
   * Source: https://github.com/Leaflet/Leaflet/issues/6113#issuecomment-377672239
   * @param coords Cordinates
   * @param err Error
   * @param tile Tile object?
   */
  public _tileReady(coords: Coords, err: any, tile: any) {
    if (!this._map || tile.getAttribute('src') === EmptyImage) { return; } // Replace emptyImageUrl

    if (err) {
      // @event tileerror: TileErrorEvent
      // Fired when there is an error loading a tile.
      this.fire('tileerror', {
        error: err,
        tile: tile,
        coords: coords
      });
    }

    var key = this._tileCoordsToKey(coords);

    tile = this._tiles[key];
    if (!tile) { return; }

    tile.loaded = +new Date();
    if ((this._map as any)._fadeAnimated) {
      L.DomUtil.setOpacity(tile.el, 0);
      L.Util.cancelAnimFrame((this as any)._fadeFrame);
      (this as any)._fadeFrame = L.Util.requestAnimFrame((this as any)._updateOpacity, this);
    } else {
      tile.active = true;
      (this as any)._pruneTiles();
    }

    if (!err) {
      L.DomUtil.addClass(tile.el, 'leaflet-tile-loaded');

      // @event tileload: TileEvent
      // Fired when a tile loads.
      this.fire('tileload', {
        tile: tile.el,
        coords: coords
      });
    }

    if ((this as any)._noTilesToLoad()) {
      (this as any)._loading = false;
      // @event load: Event
      // Fired when the grid layer loaded all visible tiles.
      this.fire('load');

      if (L.Browser.ielt9 || !(this as any)._map._fadeAnimated) {
        L.Util.requestAnimFrame((this as any)._pruneTiles, this);
      } else {
        // Wait a bit more than 0.2 secs (the duration of the tile fade-in)
        // to trigger a pruning.
        setTimeout(L.Util.bind((this as any)._pruneTiles, this), 250);
      }
    }
  }
}


export default LeafletEmptyImageUrlGridLayer;
