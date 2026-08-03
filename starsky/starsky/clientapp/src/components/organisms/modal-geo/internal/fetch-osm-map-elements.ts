import FetchGet from "../../../../shared/fetch/fetch-get";
import { UrlQuery } from "../../../../shared/url/url-query";

export interface IOsmMapElementItem {
  id?: string;
  elementType?: string;
  label?: string;
  category?: string;
  type?: string;
  description?: string;
  copyText?: string;
  distanceMeters?: number;
}

export interface IOsmMapElementsResult {
  error?: string;
  nearbyObjects: IOsmMapElementItem[];
  enclosingObjects: IOsmMapElementItem[];
}

/**
 * Fetches nearby and enclosing OSM map elements from backend lookup endpoint.
 */
export async function FetchMapElementsFromOsm(
  latitude: number,
  longitude: number
): Promise<IOsmMapElementsResult | null> {
  try {
    const url = new UrlQuery().UrlGeoMapElements(latitude, longitude);
    const result = await FetchGet(url, { "User-Agent": "Starsky-App" });
    if (result.statusCode !== 200 || !result.data) {
      console.error("OSM map elements API request failed:", result.statusCode);
      return null;
    }

    const data = result.data as IOsmMapElementsResult;
    return {
      error: data.error,
      nearbyObjects: data.nearbyObjects || [],
      enclosingObjects: data.enclosingObjects || []
    };
  } catch (error) {
    console.error("Error fetching OSM map elements:", error);
    return null;
  }
}
