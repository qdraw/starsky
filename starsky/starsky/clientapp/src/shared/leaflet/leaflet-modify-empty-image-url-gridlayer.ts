import L, { Coords, GridLayer } from "leaflet";
import { EmptyImageUrl } from "./empty-image.const";

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

  public init() {
    return true;
  }

  public _removeTile(key: string) {
    const tile = this._tiles[key];
    if (!tile) {
      return;
    }

    // Cancels any pending http requests associated with the tile
    // unless we're on Android's stock browser,
    // see https://github.com/Leaflet/Leaflet/issues/137
    if (!L.Browser.androidStock) {
      tile.el.setAttribute("src", EmptyImageUrl); // Replace emptyImageUrl
    }
    L.DomUtil.remove(tile.el);

    delete this._tiles[key];

    // @event tileunload: TileEvent
    // Fired when a tile is removed (e.g. when a tile goes off the screen).
    this.fire("tileunload", {
      tile: tile.el,
      coords: (this as unknown as { _keyToTileCoords: (key: string) => void })._keyToTileCoords(key)
    });
  }

  /**
   * When Tile is ready, this is fired by leaflet
   * Source: https://github.com/Leaflet/Leaflet/issues/6113#issuecomment-377672239
   * @param coords Cordinates
   * @param err Error
   * @param tile Tile object?
   */
  public _tileReady(
    coords: Coords,
    err: boolean | null,
    tile: {
      active?: boolean;
      coords: Coords;
      current: boolean;
      el: HTMLElement;
      loaded?: Date | number;
      retain?: boolean;
      getAttribute?: (name: string) => string | null;
    }
  ) {
    if (!this._map || (tile.getAttribute && tile.getAttribute("src") === "empty-image.gif")) {
      return;
    } // Replace emptyImageUrl

    if (err) {
      // @event tileerror: TileErrorEvent
      // Fired when there is an error loading a tile.
      this.fire("tileerror", {
        error: err,
        tile: tile,
        coords: coords
      });
    }

    const key = this._tileCoordsToKey(coords);

    tile = this._tiles[key];
    if (!tile) {
      return;
    }
    tile.loaded = +Date.now();

    if ((this._map as unknown as { _fadeAnimated: boolean })._fadeAnimated) {
      L.DomUtil.setOpacity(tile.el, 0);
      L.Util.cancelAnimFrame((this as unknown as { _fadeFrame: number })._fadeFrame);
      (this as unknown as { _fadeFrame: number })._fadeFrame = L.Util.requestAnimFrame(
        (this as unknown as { _updateOpacity: (timestamp: number) => void })._updateOpacity,
        this
      );
    } else {
      tile.active = true;
      (this as unknown as { _pruneTiles: () => void })._pruneTiles();
    }

    if (!err) {
      L.DomUtil.addClass(tile.el, "leaflet-tile-loaded");

      // @event tileload: TileEvent
      // Fired when a tile loads.
      this.fire("tileload", {
        tile: tile.el,
        coords: coords
      });
    }

    if ((this as unknown as { _noTilesToLoad: () => boolean })._noTilesToLoad()) {
      (this as unknown as { _loading: boolean })._loading = false;
      // @event load: Event
      // Fired when the grid layer loaded all visible tiles.
      this.fire("load");

      if (
        L.Browser.ielt9 ||
        !(this as unknown as { _map: { _fadeAnimated: boolean } })._map._fadeAnimated
      ) {
        L.Util.requestAnimFrame((this as unknown as { _pruneTiles: () => void })._pruneTiles, this);
      } else {
        // Wait a bit more than 0.2 secs (the duration of the tile fade-in)
        // to trigger a pruning.
        setTimeout(
          L.Util.bind((this as unknown as { _pruneTiles: () => void })._pruneTiles, this),
          250
        );
      }
    }
  }
}
