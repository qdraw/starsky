import L, { TileLayer } from "leaflet";
import { EmptyImageUrl } from "./empty-image.const";

export class LeafletEmptyImageUrlTileLayer extends TileLayer {
  /**
   * Include Strict CSP policy for TileLayer
   * You need Grid and Tile Layer
   */
  constructor(props: string) {
    super(props);
    L.TileLayer.include({ _abortLoading: this._abortLoading });
  }

  public init() {
    return true;
  }

  /**
   * Stop the loading
   * Source: https://github.com/Leaflet/Leaflet/issues/6113#issuecomment-377672239
   */
  public _abortLoading() {
    let i;
    let tile: HTMLImageElement;
    for (i in this._tiles) {
      if (this._tiles[i].coords.z !== this._tileZoom) {
        tile = this._tiles[i].el as HTMLImageElement;

        tile.onload = L.Util.falseFn;
        tile.onerror = L.Util.falseFn;
        if (!tile.complete) {
          tile.src = EmptyImageUrl; // Replace emptyImageUrl
          L.DomUtil.remove(tile);
          delete this._tiles[i];
        }
      }
    }
  }
}
