import L, { TileLayer } from 'leaflet';
import EmptyImage from '../style/images/empty-image.gif';

export class LeafletEmptyImageUrlTileLayer extends TileLayer {

  /**
   * Include Strict CSP policy for TileLayer
   * You need Grid and Tile Layer
   */
  constructor(props: string) {
    super(props);
    L.TileLayer.include({ _abortLoading: this._abortLoading });
  }

  /**
   * Stop the loading
   * Source: https://github.com/Leaflet/Leaflet/issues/6113#issuecomment-377672239
   */
  public _abortLoading() {
    var i;
    var tile: any;
    for (i in this._tiles) {
      if (this._tiles[i].coords.z !== this._tileZoom) {
        tile = this._tiles[i].el;

        tile.onload = L.Util.falseFn;
        tile.onerror = L.Util.falseFn;
        if (!tile.complete) {
          tile.src = EmptyImage; // Replace emptyImageUrl
          L.DomUtil.remove(tile);
          delete this._tiles[i];
        }
      }
    }
  }
}
export default LeafletEmptyImageUrlTileLayer;
